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

    public override bool AppendImmutableMarkup(string literal)
    {
        if (IsFinalAppend(literal) && TryInjectHttpXKernel(literal))
        {
            return true;
        }

        return base.AppendImmutableMarkup(literal);
    }

    public override bool AppendMutableValue(string value) => WriteMutableValue(value);
    public override bool AppendMutableValue(bool value) => WriteMutableValue(value ? Boolean.TrueString : Boolean.FalseString);
    public override bool AppendMutableValue<T>(T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // Wraps the mutable value with a comment tag on one side 
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

        return CompleteFormattedValue();
    }

    private bool WriteMutableValue(string value)
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

        return CompleteFormattedValue();
    }

    public override bool AppendMutableAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        var @continue = base.AppendMutableAttribute(attrName, attrValue, expression);
        Writer.Inject($" slot{Cursor}=\"{attrName}\"");

        return @continue;
    }

    public override bool AppendMutableAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var @continue = base.AppendMutableAttribute(attrName, attrValue, format, expression);
        Writer.Inject($" slot{Cursor}=\"{attrName}\"");

        return @continue;
    }

    public override bool AppendMutableAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        // Mutable attributes can't be simply wrapped like mutable values.  So instead,
        // they include a sentinel by its slot ID which indicates the
        // attribute name to references.  At the end of the body, a script
        // picks them all up and registers them in a single pass.
        // It should end up looking like this:
        //   <input type={ myType } slot123="type" />

        suppressSentinels = true;

        var @continue = base.AppendMutableAttribute(attrName, attrValue, expression);
        Writer.Inject($" slot{Cursor}=\"{attrName}\"");

        suppressSentinels = false;

        return @continue;
    }

    public override bool AppendEventHandler(Action eventHandler, string? expression = null) => AppendEventHandler(includeEventArg: false);
    public override bool AppendEventHandler(Action<Event> eventHandler, string? expression = null) => AppendEventHandler(includeEventArg: true);
    public override bool AppendEventHandler(Func<Task> eventHandler, string? expression = null) => AppendEventHandler(includeEventArg: false);
    public override bool AppendEventHandler(Func<Event, Task> eventHandler, string? expression = null) => AppendEventHandler(includeEventArg: true);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => AppendEventHandler(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => AppendEventHandler(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => AppendEventHandler(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => AppendEventHandler(attributeName);
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
        return CompleteFormattedValue();
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
        return CompleteFormattedValue();
    }

    public override bool AppendMutableElement<TView>(TView view) => AppendMutableElement(view.Render());
    public override bool AppendMutableElement(Slot slot) => AppendMutableElement(slot());
    public override bool AppendMutableElement(Html partial, string? expression = null)
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

        return CompleteFormattedValue();
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
        // If there are zero mutable slots, then we can skip this.
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
            CompleteStringLiteral(literal.Length);
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