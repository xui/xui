using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO.Pipelines;
using System.Text;
using Web4.Composers;

namespace Web4.Composers;

public class XtmlComposer(IBufferWriter<byte> writer, WindowBuilder window) : HtmlComposer(writer)
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
    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null) => WriteMutableString(ref parent, value.Name);
    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => WriteMutableString(ref parent, value.ToString()); // TODO: Memory allocation!
    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // Wraps the mutable value with a comment tag on one side 
        // to separate it from any preceding text and a script tag 
        // on the other side which registers it without the need 
        // for id= or document.getElementById().
        // It should end up looking like this:
        // $"<!-- -->{value:format}<script>key`{id}`)</script>"

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
                <script>key`{key.AsSpan()[3..]}`</script>
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
            Writer.Inject($"""<script>key`{key.AsSpan()[3..]}`</script>""");
        }
        cursor++;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null)
    {
        var @continue = base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
        Writer.Inject($" {Keymaker.GetKey(parentKey, cursor++, parent.Length)}=\"{attrName}\"");

        return @continue;
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        var @continue = base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
        Writer.Inject($" {Keymaker.GetKey(parentKey, cursor++, parent.Length)}=\"{attrName}\"");

        return @continue;
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var @continue = base.WriteMutableAttribute(ref parent, attrName, attrValue, format, expression);
        Writer.Inject($" {Keymaker.GetKey(parentKey, cursor++, parent.Length)}=\"{attrName}\"");

        return @continue;
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null)
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
        Writer.Inject($" {key}=\"{attrName}\"");

        parentKey = parent.Key;
        parentLength = parent.Length;
        cursor = parent.Cursor / 2 + 1;

        suppressSentinels = false;

        return @continue;
    }

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: false, format);
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: true, format);
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: false, format);
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, includeEventArg: true, format);
    public override bool WriteEventListener(ref Html parent, ReadOnlySpan<char> argName, Action<object> listener, string? expression = null) => WriteEventListener(ref parent, argName);
    private bool WriteEventListener(ref Html parent, bool includeEventArg, string? format = null)
    {
        if (includeEventArg && format != null)
        {
            Writer.Inject($"""
                "rpc('{Keymaker.GetKey(parentKey, cursor++, parent.Length)}',event,'{format ?? ""}')"
                """);
        }
        else if (includeEventArg && format == null)
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
    private bool WriteEventListener(ref Html parent, ReadOnlySpan<char> includedAttributeName)
    {
        Writer.Inject($"{includedAttributeName}=");
        Writer.Inject($"""
            "rpc('{Keymaker.GetKey(parentKey, cursor++, parent.Length)}')"
            """);
        // Note: When the attribute name is included as a part of the expression 
        // (e.g. $"<button { onclick => Onclick() }>Click me</button>")
        // then there's never a way to ALSO pass the event arg into the method signature.
        // For that, you'd need $"<button onclick={ e => OnClick(e) }Click me</button>">
        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html parent, TView view, string? format = null) => WriteMutableElement(ref parent, view.Render(), format);
    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null, string? format = null)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler 
        // https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        
        if (!suppressSentinels)
        {
            Writer.Inject($"""
                <script>key`{partial.Key.AsSpan()[3..]}`</script>
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
            Writer.Inject($$"""<script>ui={};function key(k){ui['key'+k[0]]=document.currentScript.previousSibling;}key`{{key.AsSpan()[3..]}}`;</script>""");
            isJsRegisterWritten = true;
            return false;
        }
        return true;
    }

    // Fear not, this Dictionary will not grow unbounded.  
    // Since it's only ever called from AppendLiteral, that means the universe of
    // possible keys is finite - only what is already compiled into the executable.
    private static readonly Dictionary<string, int> jsInjectionPoint = [];
    private static readonly string KERNEL_JS = 
        new StreamReader(System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("Web4.Kernel.js")!
        )
        .ReadToEnd();
        // .Replace("\n", "")
        // .Replace("  ", "");

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

            if (window.Listeners.Count > 0)
            {
                Writer.Inject($"{beforeBody}\n<!-- Web4 kernel injected -->\n<script>");
                foreach (var listener in window.Listeners)
                    Writer.Inject($"\n{listener.Html}");
                Writer.Inject($"\n\n{KERNEL_JS}{Debug.JS}</script>\n{afterBody}");
            }
            else
            {
                Writer.Inject($"{beforeBody}\n<!-- Web4 kernel injected -->\n<script>\n{KERNEL_JS}{Debug.JS}</script>\n{afterBody}");
            }


            suppressSentinels = true;
            CompleteStringLiteral(literal.Length);
            return true;
        }

        return false;
    }
}