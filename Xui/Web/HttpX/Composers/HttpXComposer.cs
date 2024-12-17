using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public static class HttpXComposerExtensions
{
    public static ValueTask<FlushResult> WriteAsync(this Func<Html> html, PipeWriter pipeWriter, bool sentinels = false)
    {
        if (sentinels)
        {
            var composer = new HttpXComposer(pipeWriter);
            return pipeWriter.WriteAsync(composer, $"{html()}");
        }
        else
        {
            var composer = new DefaultComposer(pipeWriter);
            return pipeWriter.WriteAsync(composer, $"{html()}");
        }
    }
}

public class HttpXComposer(IBufferWriter<byte> writer) : DefaultComposer(writer)
{
    private bool isJsRegisterWritten = false;
    private bool suppressSentinels = false;

    public override bool AppendStaticPartialMarkup(string literal)
    {
        if (IsFinalAppend(literal) && TryInjectHttpXKernel(literal))
        {
            return true;
        }

        return base.AppendStaticPartialMarkup(literal);
    }

    public override bool AppendDynamicValue(string value) => WriteDynamicValue(value);
    public override bool AppendDynamicValue(bool value) => WriteDynamicValue(value ? Boolean.TrueString : Boolean.FalseString);
    public override bool AppendDynamicValue<T>(T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // Wraps the dynamic value with a comment tag on one side 
        // to separate it from any preceding text and a script tag 
        // on the other side which registers it without the need 
        // for id= or document.getElementById().
        // It should end up looking like this:
        // $"<!-- -->{value:format}<script>r('slot{id}')</script>"

        if (!suppressSentinels)
        {
            Writer.Inject($"<!-- -->");
        }

        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        if (!suppressSentinels && EnsureJsRegisterIsWritten())
        {
            Writer.Inject($"""
                <script>r("slot{Cursor}")</script>
                """);
        }

        return CompleteDynamic(1);
    }

    private bool WriteDynamicValue(string value)
    {
        if (!suppressSentinels)
        {
            Writer.Inject($"<!-- -->{value}");
        }
        else
        {
            Encoding.UTF8.GetBytes(value, Writer);
        }

        if (!suppressSentinels && EnsureJsRegisterIsWritten())
        {
            Writer.Inject($"""<script>r("slot{Cursor}")</script>""");
        }

        return CompleteDynamic(1);
    }

    public override bool AppendDynamicAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue)
    {
        var @continue = base.AppendDynamicAttribute(attrName, attrValue);
        Writer.Inject($" slot{Cursor}=\"{attrName}\"");

        return @continue;
    }

    public override bool AppendDynamicAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var @continue = base.AppendDynamicAttribute(attrName, attrValue, format);
        Writer.Inject($" slot{Cursor}=\"{attrName}\"");

        return @continue;
    }

    public override bool AppendDynamicAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue)
    {
        // Attributes can't be wrapped like dynamic values.  So instead,
        // they include a sentinel by its slot ID which indicates the
        // attribute name to references.  At the end of the body, a script
        // picks them all up and registers them in a single pass.
        // It should end up looking like this:
        //   <input type={ myType } slot123="type" />

        suppressSentinels = true;

        var @continue = base.AppendDynamicAttribute(attrName, attrValue);
        Writer.Inject($" slot{Cursor}=\"{attrName}\"");

        suppressSentinels = false;

        return @continue;
    }

    public override bool AppendEventHandler(Action eventHandler) => AppendEventHandler(includeEventArg: false);
    public override bool AppendEventHandler(Action<Event> eventHandler) => AppendEventHandler(includeEventArg: true);
    public override bool AppendEventHandler(Func<Task> eventHandler) => AppendEventHandler(includeEventArg: false);
    public override bool AppendEventHandler(Func<Event, Task> eventHandler) => AppendEventHandler(includeEventArg: true);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action eventHandler) => AppendEventHandler(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action<Event> eventHandler) => AppendEventHandler(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Task> eventHandler) => AppendEventHandler(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler) => AppendEventHandler(attributeName);
    private bool AppendEventHandler(bool includeEventArg)
    {
        if (includeEventArg)
        {
            Writer.Inject($"""
                "h({Cursor},event)"
                """);
        }
        else
        {
            Writer.Inject($"""
                "h({Cursor})"
                """);
        }
        return CompleteDynamic(1);
    }
    private bool AppendEventHandler(ReadOnlySpan<char> includedAttributeName)
    {
        Writer.Inject($"{includedAttributeName}=");
        Writer.Inject($"""
            "h({Cursor})"
            """);
        // Note: When the attribute name is included as a part of the expression 
        // (e.g. $"<button { onclick => c++ }>Click me</button>")
        // then there's never a way to ALSO pass the event arg into the method signature.
        // For that, you'd need $"<button onclick={ e => c++ }Click me</button>">
        return CompleteDynamic(1);
    }

    public override bool AppendDynamicElement<TView>(TView view) => AppendDynamicElement(view.Render());
    public override bool AppendDynamicElement(Slot slot) => AppendDynamicElement(slot());
    public override bool AppendDynamicElement(Html partial)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler 
        // https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        
        if (!suppressSentinels)
        {
            Writer.Inject($"""
                <script>r("slot{Cursor}")</script>
                """);
        }

        return CompleteDynamic(1);
    }

    protected override void Clear()
    {
        // Set to false in case this composer instance is reused.
        isJsRegisterWritten = false;
        suppressSentinels = false;

        base.Clear();
    }

    private bool EnsureJsRegisterIsWritten()
    {
        if (!isJsRegisterWritten)
        {
            Writer.Inject($"{JS_REGISTER}");
            isJsRegisterWritten = true;
            return false;
        }
        return true;
    }

    private static readonly string JS_REGISTER = """
        <script>
            ui={};
            function r(k) {
                ui[k]=document.currentScript.previousSibling;
            }
            r('slot0');
        </script>
        """
        .Replace("\n", "")
        .Replace("  ", "");

    // Fear not, this Dictionary will not grow unbounded.  
    // Since it's only ever called from AppendLiteral, that means the universe of
    // possible keys is finite - only what is already compiled into the executable.
    private static readonly Dictionary<string, int> jsInjectionPoint = [];

    private bool TryInjectHttpXKernel(string literal)
    {
        // If there are zero dynamic slots, then we can skip this.
        if (FormattedCount <= 1)
        {
            suppressSentinels = true;
            return false;
        }

        // Inject the necessary JavaScript before the end of the </body> tag.
        if (!jsInjectionPoint.TryGetValue(literal, out int index))
        {
            // Avoid expensive operation?
            index = literal.IndexOf("</body>");
            jsInjectionPoint[literal] = index;
        }
        if (index >= 0)
        {
            var beforeBody = literal.AsSpan(0, index);
            var afterBody = literal.AsSpan(index, literal.Length - index);
            Writer.Inject($"{beforeBody}{JS}{afterBody}");

            suppressSentinels = true;
            CompleteStatic(literal.Length);
            return true;
        }

        return false;
    }

    private static readonly string JS = """
        <script>
            var ui=ui??{};
            for (let k in ui) {
                let n = ui[k];
                if (n.nodeType == 8) {
                    let t = document.createTextNode("");
                    n.parentNode.insertBefore(t, n.nextSibling);
                    ui[k]=t;
                }
            }
            var attrs = document.evaluate('//*/attribute::*[starts-with(name(), "slot")]', document, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
            for (i=0;i<attrs.snapshotLength;i++) {
                let a=attrs.snapshotItem(i);
                ui[a.name]=a;
            }

            function h(id,ev) {
                console.debug("executing slot " + id);
                if (ev) {
                    ws.send(`${id}${encodeEvent(ev)}`);
                } else {
                    ws.send(id);
                }
            }

            function debugSocket(name, ws) {
                ws.onopen = (event) => { console.debug(`${name} onopen`, event); };
                ws.onclose = (event) => { console.debug(`${name} onclose`, event); };
                ws.onerror = (event) => { console.error(`${name} onerror`, event); };
            }

            var l = location;
            const ws = new WebSocket(`ws://${l.host}${l.pathname}`);
            debugSocket("xui", ws);
            ws.onmessage = (event) => {
                console.debug("onmessage: ", event);
                eval(event.data);
            };

            function encodeEvent(e) {
                const obj = {};
                for (let k in e) { obj[k] = e[k]; }
                return JSON.stringify(obj, (k, v) => {
                    /* TODO: There are a few more properties that can be shaved off. */
                    if (v instanceof Node) return {id: v.id,name: v.name,type: v.type,value: v.value};
                    if (v instanceof Window) return null;
                    return v;
                }, '');
            }

            function replaceNode(node, content) {
                const regScript = node.nextSibling;
                node.outerHTML = content;
                node = regScript.previousSibling;
                reRegister(regScript);
                for (let s of node.getElementsByTagName("script")) {
                    reRegister(s);
                }
            }

            function reRegister(node) {
                if (node.tagName == "SCRIPT") {
                    const s = document.createElement("script");
                    s.textContent = node.textContent;
                    node.replaceWith(s);
                }
            }
        </script>
        """
        .Replace("\n", "")
        .Replace("  ", "");
}