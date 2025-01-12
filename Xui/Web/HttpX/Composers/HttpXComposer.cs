using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class HttpXComposer(IBufferWriter<byte> writer) : DefaultComposer(writer)
{
    private string parentKey = string.Empty;
    private int parentLength = 0;
    private int cursor = 0;
    private bool isJsRegisterWritten = false;
    private bool suppressSentinels = false;

    protected override void Clear()
    {
        parentKey = string.Empty;
        parentLength = 0;
        cursor = 0;
        
        isJsRegisterWritten = false;
        suppressSentinels = false;

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        // Skip the root.  It doesn't need a key.
        html.Key = IsInitialAppend()
            ? string.Empty
            : Keymaker.GetKey(parentKey, cursor++, parentLength);
        parentKey = html.Key;
        parentLength = html.Length;
        cursor = 0;
        
        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        if (IsFinalAppend(literal) && TryInjectHttpXKernel(literal))
        {
            return true;
        }

        return base.WriteImmutableMarkup(ref parent, literal);
    }

    public override bool WriteMutableValue(ref Html parent, string value) => WriteMutableString(ref parent, value);
    public override bool WriteMutableValue(ref Html parent, bool value) => WriteMutableString(ref parent, value ? Boolean.TrueString : Boolean.FalseString);
    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // Wraps the mutable value with a comment tag on one side 
        // to separate it from any preceding text and a script tag 
        // on the other side which registers it without the need 
        // for id= or document.getElementById().
        // It should end up looking like this:
        // $"<!-- -->{value:format}<script>r('key{id}')</script>"

        if (!suppressSentinels)
        {
            Writer.Inject($"<!-- -->");
        }

        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        var key = Keymaker.GetKey(parentKey, cursor, parent.Length);
        if (!suppressSentinels && EnsureJsRegisterIsWritten(key))
        {
            Writer.Inject($"""
                <script>reg('key{key}')</script>
                """);
        }
        cursor++;

        return CompleteFormattedValue();
    }

    private bool WriteMutableString(ref Html parent, string value)
    {
        if (!suppressSentinels)
        {
            Writer.Inject($"<!-- -->{value}");
        }
        else
        {
            Encoding.UTF8.GetBytes(value, Writer);
        }

        var key = Keymaker.GetKey(parentKey, cursor, parent.Length);
        if (!suppressSentinels && EnsureJsRegisterIsWritten(key))
        {
            Writer.Inject($"""<script>reg('key{key}')</script>""");
        }
        cursor++;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        var @continue = base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
        Writer.Inject($" key{Keymaker.GetKey(parentKey, cursor++, parent.Length)}=\"{attrName}\"");

        return @continue;
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var @continue = base.WriteMutableAttribute(ref parent, attrName, attrValue, format, expression);
        Writer.Inject($" key{Keymaker.GetKey(parentKey, cursor++, parent.Length)}=\"{attrName}\"");

        return @continue;
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        // Mutable attributes can't be simply wrapped like mutable values.  So instead,
        // they include a sentinel by its key ID which indicates the
        // attribute name to references.  At the end of the body, a script
        // picks them all up and registers them in a single pass.
        // It should end up looking like this:
        //   <input type={ myType } keyAB="type" />

        suppressSentinels = true;

        var key = Keymaker.GetKey(parentKey, cursor++, parent.Length);

        var @continue = base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
        Writer.Inject($" key{key}=\"{attrName}\"");

        parentKey = parent.Key;
        parentLength = parent.Length;
        cursor = parent.Cursor / 2 + 1;

        suppressSentinels = false;

        return @continue;
    }

    public override bool WriteEventHandler(ref Html parent, Action eventHandler, string? expression = null) => WriteEventHandler(ref parent, includeEventArg: false);
    public override bool WriteEventHandler(ref Html parent, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref parent, includeEventArg: true);
    public override bool WriteEventHandler(ref Html parent, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, includeEventArg: false);
    public override bool WriteEventHandler(ref Html parent, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, includeEventArg: true);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => WriteEventHandler(ref parent, attributeName);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref parent, attributeName);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, attributeName);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, attributeName);
    private bool WriteEventHandler(ref Html parent, bool includeEventArg)
    {
        if (includeEventArg)
        {
            Writer.Inject($"""
                "rpc('{Keymaker.GetKey(parentKey, cursor++, parent.Length)}',event)"
                """);
        }
        else
        {
            Writer.Inject($"""
                "rpc('{Keymaker.GetKey(parentKey, cursor++, parent.Length)}')"
                """);
        }
        return CompleteFormattedValue();
    }
    private bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> includedAttributeName)
    {
        Writer.Inject($"{includedAttributeName}=");
        Writer.Inject($"""
            "rpc('{Keymaker.GetKey(parentKey, cursor++, parent.Length)}')"
            """);
        // Note: When the attribute name is included as a part of the expression 
        // (e.g. $"<button { onclick => c++ }>Click me</button>")
        // then there's never a way to ALSO pass the event arg into the method signature.
        // For that, you'd need $"<button onclick={ e => c++ }Click me</button>">
        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html parent, TView view) => WriteMutableElement(ref parent, view.Render());
    public override bool WriteMutableElement(ref Html parent, Slot slot) => WriteMutableElement(ref parent, slot());
    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler 
        // https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        
        if (!suppressSentinels)
        {
            Writer.Inject($"""
                <script>reg('key{partial.Key}')</script>
                """);
        }

        parentKey = parent.Key;
        parentLength = parent.Length;
        cursor = parent.Cursor / 2 + 1;

        return CompleteFormattedValue();
    }

    private bool EnsureJsRegisterIsWritten(string key)
    {
        if (!isJsRegisterWritten)
        {
            Writer.Inject($$"""<script>ui={};function reg(k){ui[k]=document.currentScript.previousSibling;}reg('key{{key}}');</script>""");
            isJsRegisterWritten = true;
            return false;
        }
        return true;
    }

    // Fear not, this Dictionary will not grow unbounded.  
    // Since it's only ever called from AppendLiteral, that means the universe of
    // possible keys is finite - only what is already compiled into the executable.
    private static readonly Dictionary<string, int> jsInjectionPoint = [];

    private bool TryInjectHttpXKernel(string literal)
    {
        // If there are zero mutable keys, then we can skip this.
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
            var attrs = document.evaluate('//*/attribute::*[starts-with(name(), "key")]', document, null, XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
            for (i=0;i<attrs.snapshotLength;i++) {
                let a=attrs.snapshotItem(i);
                ui[a.name]=a;
            }

            function rpc(key,ev) {
                let body = (ev) ? encodeEvent(ev) : '';
                let command = 
        `CALL /app#key${key} XTTP/0.1
        Content-Type: application/json
        Content-Length: ${body.length}

        ${body}`;
                ws.send(command);
            }

            function debugSocket(name, ws) {
            }

            var l = location;
            const ws = new WebSocket(`ws://${l.host}${l.pathname}`);
            ws.onopen = (event) => { console.debug(`${name} onopen`, event); };
            ws.onclose = (event) => { console.debug(`${name} onclose`, event); };
            ws.onerror = (event) => { console.error(`${name} onerror`, event); };
            ws.onmessage = (event) => {
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
        """;
        // .Replace("\n", "")
        // .Replace("  ", "");
}