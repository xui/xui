using System.Buffers;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public class XtmlComposer(IBufferWriter<byte> writer, WindowBuilder window)
    : KeyholeComposer, IStreamingComposer
{
    private enum AttributeStatus { None, Pending, InProgress }
    private AttributeStatus attributeStatus = AttributeStatus.None;
    private ReadOnlyMemory<char>? deferredLiteral = null;
    private bool isBodyOmitted = false;

    public IBufferWriter<byte> Writer { get; set; } = writer;
    public WindowBuilder Window { get; set; } = window;

    public override bool OnTemplateBegin(ref Html html, ref string literal)
    {
        InjectBootloader(ref literal);

        return true;
    }

    public override bool OnTemplateEnd(ref Html html)
    {
        if (isBodyOmitted)
        {
            Writer.Write("""
                    
                    </body>
                </html>
                """u8);
        }

        return true;
    }

    public override bool OnMarkup(ref Html parent, string literal)
    {
        base.OnMarkup(ref parent, literal);

        // This makes the assumption that keyholes preceeded with an '=' are always attributes.  
        // Attributes need different sentinels than regular keyholes and boolean attributes 
        // have a few strange rules to follow:
        // https://developer.mozilla.org/en-US/docs/Glossary/Boolean/HTML
        if (literal.EndsWith('='))
        {
            attributeStatus = AttributeStatus.Pending;
            deferredLiteral = literal.AsMemory();
            return true;
        }

        Writer.Write(literal);

        return true;
    }

    public override bool OnStringKeyhole(ref Html parent, string value)
    {
        base.OnStringKeyhole(ref parent, value);

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{value}<!--/{key}-->`
                Writer.Write("<!--"u8, Key, "-->"u8);
                Writer.Write(value);
                Writer.Write("<!--/"u8, Key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"{value}" {key}`
                Writer.Write("\""u8);
                Writer.Write(value);
                Writer.Write("\" "u8);
                Writer.Write(Key);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                Writer.Write(value);
                break;
        }

        return true;
    }

    public override bool OnBoolKeyhole(ref Html parent, bool value)
    {
        base.OnBoolKeyhole(ref parent, value);

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{b}<!--/{key}-->`
                Writer.Write("<!--"u8, Key, "-->"u8);
                Writer.Write(value ? "true" : "false");
                Writer.Write("<!--/"u8, Key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                var attributeName = HandleDeferredLiteral(isBooleanAttribute: true);
                if (value)
                {
                    // ex: ` {attributeName}`
                    Writer.Write(" "u8);
                    Writer.Write(attributeName);
                }
                // ex: ` {key}="{attributeName}"`
                Writer.Write(" "u8);
                Writer.Write(Key);
                Writer.Write("=\""u8);
                Writer.Write(attributeName);
                Writer.Write("\""u8);

                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                Writer.Write(value ? "true" : "false");
                break;
        }

        return true;
    }

    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    private bool OnUtf8SpanFormattable<T>(ref Html parent, T value, string? format = null)
        where T : struct, IUtf8SpanFormattable
    {
        // Wraps the mutable value with two comment tags
        // to separate it from any neighboring text.
        // At the end of the body an inline script registers them 
        // because we can't rely on id= or document.getElementById().
        // It should end up looking like this:
        // $"<!--key123-->{value:format}<!--/key123-->"

        base.OnKeyhole();

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{value:format}<!--/{key}-->`
                Writer.Write("<!--"u8, Key, "-->"u8);
                Writer.Write(value, format);
                Writer.Write("<!--/"u8, Key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"{value:format}" {key}`
                Writer.Write("\""u8);
                Writer.Write(value, format);
                Writer.Write("\" "u8);
                Writer.Write(Key);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                Writer.Write(value, format);
                break;
        }

        return true;
    }

    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null)
    {
        base.OnColorKeyhole(ref parent, value, format);

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{value:format}<!--/{key}-->`
                Writer.Write("<!--"u8, Key, "-->"u8);
                Writer.Write(value, format);
                Writer.Write("<!--/"u8, Key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"{value:format}" {key}`
                Writer.Write("\""u8);
                Writer.Write(value, format);
                Writer.Write("\" "u8);
                Writer.Write(Key);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                Writer.Write(value, format);
                break;
        }

        return true;
    }

    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null)
        => OnStringKeyhole(ref parent, value.ToString()); // TODO: Memory allocation!
        
    public override bool OnHtmlBegin(ref Html html)
    {
        base.OnHtmlBegin(ref html);

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->`
                Writer.Write("<!--"u8, Key, "-->"u8);
                break;
            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"` (the value will come later in the next On*Keyhole())
                Writer.Write("\""u8);
                attributeStatus = AttributeStatus.InProgress;
                break;
        }

        return true;
    }

    public override bool OnHtmlEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        base.OnHtmlEnd(ref parent, html, format, expression);

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--/{key}-->`
                Writer.Write("<!--/"u8, Key, "-->"u8);
                if (format is {} transition)
                    InjectTransition(Key, transition);
                break;

            case AttributeStatus.InProgress:
                // ex: `" {key}`
                Writer.Write("\" "u8);
                Writer.Write(Key);
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.Pending:
                throw new NotSupportedException("Attributes cannot have nested Htmls");
        }

        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        base.OnIteratorBegin(ref parent, ref htmls, format, expression);
        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        base.OnIteratorEnd(ref parent, ref htmls, format, expression);
        
        // Keyhole to represent the loop itself, useful for zero-length use cases.
        // ex: `<!--{key} /-->`
        Writer.Write("<!--"u8, Key, " /-->"u8);

        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: false, format);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: true, format);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: false, format);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: true, format);
    private bool OnListener(ref Html parent, bool includeEventArg, string? format = null)
    {
        base.OnKeyhole();

        if (deferredLiteral != null)
            HandleDeferredLiteral();

        if (includeEventArg)
        {
            // ex: `"keyholes.{key}.dispatchEvent(event.trim('{format ?? "*"}'))" {key}`
            Writer.Write("\"keyholes."u8);
            Writer.Write(Key);
            Writer.Write(".dispatchEvent(event.trim('"u8);
            Writer.Write(format ?? "*");
            Writer.Write("'))\" "u8);
            Writer.Write(Key);
        }
        else
        {
            // TODO: Is it better to use ({}) or () instead?
            // ex: `"keyholes.{key}.dispatchEvent(event.trim(''))" {key}`
            Writer.Write("\"keyholes."u8);
            Writer.Write(Key);
            Writer.Write(".dispatchEvent(event.trim(''))\" "u8);
            Writer.Write(Key);
        }
        
        attributeStatus = AttributeStatus.None;
        return true;
    }

    private void HandleDeferredLiteral()
    {
        if (!deferredLiteral.HasValue)
            throw new NullReferenceException(nameof(deferredLiteral));

        Writer.Write(deferredLiteral.Value);
        deferredLiteral = null;
    }

    private ReadOnlySpan<char> HandleDeferredLiteral(bool isBooleanAttribute = true)
    {
        if (!isBooleanAttribute)
        {
            HandleDeferredLiteral();
            return [];
        }

        if (!deferredLiteral.HasValue)
            throw new NullReferenceException(nameof(deferredLiteral));

        // This string literal will look something like `...<input type="checkbox" checked=`
        // Note: We know they always end with `=`.
        var deferredLiteralSpan = deferredLiteral.Value.Span;
        int indexBeforeAttribute = deferredLiteralSpan.LastIndexOf(' ');
        ArgumentOutOfRangeException.ThrowIfLessThan(indexBeforeAttribute, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(indexBeforeAttribute, deferredLiteralSpan.Length - 2);

        Writer.Write(deferredLiteralSpan[..indexBeforeAttribute]);
        var attributeName = deferredLiteralSpan[(indexBeforeAttribute + 1)..^1];

        deferredLiteral = null;
        return attributeName;
    }

    private static readonly byte[] BOOTLOADER = 
        Encoding.UTF8.GetBytes(new StreamReader(System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("Web4.Bootloader.html")!
        ).ReadToEnd());

    private void InjectBootloader(ref string literal)
    {
        Writer.Write("""
            <!doctype html>
            <html>
            <head>
            
            """u8);

        // Write dev-included <head> content (if any)
        int headStart = literal.IndexOf("<head>", StringComparison.Ordinal);
        if (headStart >= 0)
        {
            headStart += 6; // "<head>".Length;
            int headEnd = literal.IndexOf("</head>", headStart, StringComparison.Ordinal);
            if (headEnd > headStart)
                Writer.Write(literal.AsSpan(headStart..headEnd));
        }

        // Write necesary JavaScript and CSS to operate Web4
        Writer.Write(BOOTLOADER);

        // Write event handlers set on window or document
        if (Window.Listeners.Count > 0)
        {
            Writer.Write("\n\n<script>\n"u8);

            foreach (var listener in Window.Listeners)
            {
                // ex: `  {listener.Html}\n`
                Writer.Write("  "u8);
                Writer.Write(listener.Html ?? "");
                Writer.Write("\n"u8);
            }

            Writer.Write("</script>\n\n"u8);
        }

        // Locate the start of the <body> tag (if present)
        int bodyStart = literal.IndexOf("<body", Math.Max(headStart, 0), StringComparison.Ordinal);
        this.isBodyOmitted = bodyStart < 0;
        Writer.Write(isBodyOmitted ? "\n</head><body>\n"u8 : "\n</head>\n"u8);

        // Pre-handle the work of OnMarkup, except consider `offset`.
        // Then set `literal` to "" so the next OnMarkup no-ops.
        int offset = isBodyOmitted ? 0 : bodyStart;
        if (literal.EndsWith('='))
        {
            attributeStatus = AttributeStatus.Pending;
            deferredLiteral = literal.AsMemory(offset);
        }
        else
        {
            Writer.Write(literal.AsSpan(offset));
        }

        literal = string.Empty;
    }

    private void InjectTransition(byte[] key, string transition)
    {
        Writer.WriteRaw($$"""
            <style>
                ::view-transition-group(web4-fwd-{{key}}, web4-rev-{{key}}) { animation: none; }
                ::view-transition-new(web4-fwd-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in; }
                ::view-transition-old(web4-fwd-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out; }
                ::view-transition-new(web4-rev-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out reverse; }
                ::view-transition-old(web4-rev-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in reverse; }
            </style>
            """);
    }

    public override void Reset()
    {
        Writer = null!;
        Window = null!;
        attributeStatus = AttributeStatus.None;
        base.Reset();
    }

    [ThreadStatic] static XtmlComposer? reusable;
    public static XtmlComposer Reuse(IBufferWriter<byte> writer, WindowBuilder window) 
    {
        if (reusable is {} composer)
        {
            composer.Writer = writer;
            composer.Window = window;
            return composer;
        }

        return reusable = new XtmlComposer(writer, window);
    }
}