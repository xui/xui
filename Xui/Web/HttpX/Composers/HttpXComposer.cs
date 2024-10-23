using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class HttpXComposer(IBufferWriter<byte> writer) : DefaultComposer(writer)
{
    private bool isJsRegisterWritten = false;
    private bool suppressSentinels = false;

    public override bool AppendLiteral(string literal)
    {
        if (IsFinalAppend(literal) && TryInjectHttpXKernel(literal))
        {
            return true;
        }

        return base.AppendLiteral(literal);
    }

    public override bool AppendFormatted(string value) => WriteDynamicValue(value);

    public override bool AppendFormatted<T>(T value, string? format = default)
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

        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        if (!suppressSentinels && EnsureJsRegisterIsWritten())
        {
            Writer.Inject($"""<script>r("slot{Cursor}")</script>""");
        }

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(bool value) => WriteDynamicValue(value ? Boolean.TrueString : Boolean.FalseString);

    private bool WriteDynamicValue(string value)
    {
        if (!suppressSentinels)
        {
            Writer.Inject($"<!-- -->{value}");
        }
        else
        {
            Writer.Inject($"{value}");
        }

        if (!suppressSentinels && EnsureJsRegisterIsWritten())
        {
            Writer.Inject($"""<script>r("slot{Cursor}")</script>""");
        }

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<Event, Html> attribute, string? expression = null)
    {
        suppressSentinels = true;

        var name = GetAttributeName(expression);
        Writer.Inject($"{name}=\"");
        attribute(Event.Empty);
        Writer.Inject($"\" slot{Cursor}=\"{name}\"");

        suppressSentinels = false;

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (TryAppendAsEventHandler(name))
        {
            return CompleteDynamic(1);
        }
        
        var value = attribute(Event.Empty);
        Writer.Inject($"{name}=\"{value}\" slot{Cursor}=\"{name}\"");

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<Event, bool> attribute, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (TryAppendAsEventHandler(name))
        {
            return CompleteDynamic(1);
        }

        var value = attribute(Event.Empty);
        if (value)
        {
            Writer.Inject($"{name}");
        }

        Writer.Inject($" slot{Cursor}=\"{name}\"");

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Action eventHandler, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (TryAppendAsEventHandler(name))
        {
            return CompleteDynamic(1);
        }

        Writer.Inject($"""
            "h({Cursor})"
            """);

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Action<Event> eventHandler, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (TryAppendAsEventHandler(name))
        {
            return CompleteDynamic(1);
        }

        Writer.Inject($"""
            "h({Cursor},event)"
            """);

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<Task> eventHandler, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (TryAppendAsEventHandler(name))
        {
            return CompleteDynamic(1);
        }

        Writer.Inject($"""
            "h({Cursor})"
            """);

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<Event, Task> eventHandler, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (TryAppendAsEventHandler(name))
        {
            return CompleteDynamic(1);
        }

        Writer.Inject($"""
            "h({Cursor},event)"
            """);

        return CompleteDynamic(1);
    }

    private bool TryAppendAsEventHandler(ReadOnlySpan<char> name)
    {
        // We have an unfortunate edge case to handle here.  
        // The notation used for some attributes:
        //   $"<input type="text" { maxlength => c } />"
        // technically also matches the signature used for events:
        //   $"<button { onclick => c++ }>click me</button>"
        // Fortunately there's a simple workaround.  Since attributes
        // only use the input param for its name, never its value 
        // we can just key off its name and send it down a different path
        // as if it were an event handler.

        if (IsReservedForEvent(name))
        {
            Writer.Inject($"""
                "h({Cursor},event)"
                """);
            return true;
        }

        if (IsReservedForEventHandler(name))
        {
            Writer.Inject($"""
                {name}="h({Cursor})"
                """);         
            return true;
        }

        return false;
    }

    public override bool AppendFormatted<TView>(TView view) => AppendFormatted(view.Render());
    public override bool AppendFormatted(Slot slot) => AppendFormatted(slot());
    public override bool AppendFormatted(Html partial)
    {
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
            app={};
            function r(k) {
                app[k]=document.currentScript.previousSibling;
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
            for (let k in app) {
                let n = app[k];
                if (n.nodeType == 8) {
                    let t = document.createTextNode("");
                    n.parentNode.insertBefore(t, n.nextSibling);
                    app[k]=t;
                }
            }
            var attrs = document.evaluate('//*/attribute::*[starts-with(name(), "slot")]', document, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
            for (i=0;i<attrs.snapshotLength;i++) {
                let a=attrs.snapshotItem(i);
                app[a.name]=a;
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