using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class HttpXComposer(IBufferWriter<byte> writer) : DefaultComposer(writer)
{
    // Fear not, this Dictionary will not grow unbounded.  
    // Since it's only ever called from AppendLiteral, that means the universe of
    // possible keys is finite - only what is already compiled into the executable.
    private static readonly Dictionary<string, int> jsInjectionIndexes = [];

    private bool isJsRegisterWritten = false;
    private bool suppressDynamicValues = false;

    public override bool AppendLiteral(string s)
    {
        // Inject the necessary JavaScript before the end of the </body> tag.
        if (IsFinalAppend(s))
        {
            if (!jsInjectionIndexes.TryGetValue(s, out int index))
            {
                // Avoid expensive operation?
                index = s.IndexOf("</body>");
                jsInjectionIndexes[s] = index;
            }
            if (index >= 0)
            {
                var beforeBody = s.AsSpan(0, index);
                var afterBody = s.AsSpan(index, s.Length - index);
                Writer.WriteRaw($"{beforeBody}{JS}{afterBody}");

                suppressDynamicValues = true;

                CompleteStatic(s.Length);
                return true;
            }

            return base.AppendLiteral(s);
        }

        return base.AppendLiteral(s);
    }

    public override bool AppendFormatted(string s) => WriteDynamicValue(s);
    public override bool AppendFormatted(int i, string? format = null) => WriteDynamicValue(i, format);
    public override bool AppendFormatted(long l, string? format = null) => WriteDynamicValue(l, format);
    public override bool AppendFormatted(float f, string? format = null) => WriteDynamicValue(f, format);
    public override bool AppendFormatted(double d, string? format = null) => WriteDynamicValue(d, format);
    public override bool AppendFormatted(decimal d, string? format = null) => WriteDynamicValue(d, format);
    public override bool AppendFormatted(DateTime d, string? format = null) => WriteDynamicValue(d, format);
    public override bool AppendFormatted(TimeSpan t, string? format = null) => WriteDynamicValue(t, format);
    public override bool AppendFormatted(bool b) => WriteDynamicValue(b ? Boolean.TrueString : Boolean.FalseString);

    private bool WriteDynamicValue<T>(T value, ReadOnlySpan<char> format = default)
        where T : IUtf8SpanFormattable
    {
        // Wraps the dynamic value with a comment tag on one side 
        // to separate it from any preceding text and a script tag 
        // on the other side which registers it without the need 
        // for id= or document.getElementById().
        // It should end up looking like this:
        // $"<!-- -->{value:format}<script>r('slot{id}')</script>"

        if (!suppressDynamicValues)
        {
            Writer.WriteRaw($"<!-- -->");
        }

        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        if (!suppressDynamicValues && EnsureJsRegisterIsWritten())
        {
            Writer.WriteRaw($"""<script>r("slot{Cursor}")</script>""");
        }

        return CompleteDynamic(1);
    }

    private bool WriteDynamicValue(string value)
    {
        if (!suppressDynamicValues)
        {
            Writer.WriteRaw($"<!-- -->{value}");
        }
        else
        {
            Writer.WriteRaw($"{value}");
        }

        if (!suppressDynamicValues && EnsureJsRegisterIsWritten())
        {
            Writer.WriteRaw($"""<script>r("slot{Cursor}")</script>""");
        }

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<string, Html> attribute, string? expression = null)
    {
        suppressDynamicValues = true;

        var name = GetAttributeName(expression);
        Writer.WriteRaw($"{name}=\"");
        attribute(string.Empty);
        Writer.WriteRaw($"\" slot{Cursor}={name}\"");

        suppressDynamicValues = false;

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted<T>(Func<string, T> attribute, string? format = null, string? expression = null)
    {
        var name = GetAttributeName(expression);
        var value = attribute(string.Empty);
        Writer.WriteRaw($"{name}=\"{value}\" slot{Cursor}=\"{name}\"");

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<string, bool> attribute, string? expression = null)
    {
        var name = GetAttributeName(expression);
        var value = attribute(string.Empty);

        if (value)
        {
            Writer.WriteRaw($"{name}");
        }

        Writer.WriteRaw($" slot{Cursor}=\"{name}\"");

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted<TView>(TView v) => AppendFormatted(v.Render());
    public override bool AppendFormatted(Slot s) => AppendFormatted(s());
    public override bool AppendFormatted(Html h)
    {
        if (!suppressDynamicValues)
        {
            Writer.WriteRaw($"""
                <script>r("slot{Cursor}")</script>
                """);
        }

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Action a)
    {
        Writer.WriteRaw($"""
            "h({Cursor})"
            """);

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Action<Event> a)
    {
        Writer.WriteRaw($"""
            "h({Cursor},event)"
            """);

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<Task> f)
    {
        Writer.WriteRaw($"""
            "h({Cursor})"
            """);

        return CompleteDynamic(1);
    }

    public override bool AppendFormatted(Func<Event, Task> f)
    {
        Writer.WriteRaw($"""
            "h({Cursor},event)"
            """);

        return CompleteDynamic(1);
    }

    protected override void Clear()
    {
        // Set to false in case this composer instance is reused.
        isJsRegisterWritten = false;
        suppressDynamicValues = false;

        base.Clear();
    }

    private bool EnsureJsRegisterIsWritten()
    {
        if (!isJsRegisterWritten)
        {
            Writer.WriteRaw($"{JS_REGISTER}");
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