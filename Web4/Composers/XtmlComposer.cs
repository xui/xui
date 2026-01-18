using System.Buffers;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public class XtmlComposer(IBufferWriter<byte> writer, WindowBuilder window) : HtmlComposer(writer)
{
    private enum AttributeStatus { None, Pending, InProgress }
    private readonly KeyCursor keyCursor = new();
    private AttributeStatus attributeStatus = AttributeStatus.None;
    private ReadOnlyMemory<char>? deferredLiteral = null;
    private bool isBodyOmitted = false;

    public WindowBuilder Window { get; set; } = window;

    [ThreadStatic] static XtmlComposer? reusable;
    public static XtmlComposer Reuse(IBufferWriter<byte> writer, WindowBuilder window) 
        => (reusable ??= new(writer, window)).Set(writer, window);
    
    private XtmlComposer Set(IBufferWriter<byte> writer, WindowBuilder window)
    {
        Writer = writer;
        Window = window;
        return this;
    }

    public override void Reset()
    {
        base.Reset();
        Window = null!;
        attributeStatus = AttributeStatus.None;
        keyCursor.Reset();
    }

    public override bool OnTemplateBegin(ref Html html, ref string literal)
    {
        InjectBootloader(ref literal);

        return base.OnTemplateBegin(ref html, ref literal);
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

        return base.OnTemplateEnd(ref html);
    }

    public override bool OnElementBegin(ref Html html)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current;
        keyCursor.MoveDown();

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->`
                Writer.Write("<!--"u8, key, "-->"u8);
                break;
            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"` (the value will come later in the next On*Keyhole())
                Writer.Write("\""u8);
                attributeStatus = AttributeStatus.InProgress;
                break;
        }

        return base.OnElementBegin(ref html);
    }

    public override bool OnElementEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        keyCursor.MoveUp();
        var key = keyCursor.Current;

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--/{key}-->`
                Writer.Write("<!--/"u8, key, "-->"u8);
                if (format is {} transition)
                    InjectTransition(key, transition);
                break;

            case AttributeStatus.InProgress:
                // ex: `" {key}`
                Writer.Write("\" "u8);
                Writer.Write(key);
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.Pending:
                throw new NotSupportedException("Attributes cannot have nested Htmls");
        }

        return base.OnElementEnd(ref parent, html, format, expression);
    }

    public override bool OnMarkup(ref Html parent, string literal)
    {
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

        return base.OnMarkup(ref parent, literal);
    }

    public override bool OnStringKeyhole(ref Html parent, string value)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current;

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{value}<!--/{key}-->`
                Writer.Write("<!--"u8, key, "-->"u8);
                base.OnStringKeyhole(ref parent, value);
                Writer.Write("<!--/"u8, key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"{value}" {key}`
                Writer.Write("\""u8);
                base.OnStringKeyhole(ref parent, value);
                Writer.Write("\" "u8);
                Writer.Write(key);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                return base.OnStringKeyhole(ref parent, value);
        }

        return true;
    }

    public override bool OnBoolKeyhole(ref Html parent, bool value)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current;

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{b}<!--/{key}-->`
                Writer.Write("<!--"u8, key, "-->"u8);
                base.OnBoolKeyhole(ref parent, value);
                Writer.Write("<!--/"u8, key, "-->"u8);
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
                Writer.Write(key);
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
                return base.OnBoolKeyhole(ref parent, value);
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

        keyCursor.MoveNext();
        var key = keyCursor.Current;

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{value:format}<!--/{key}-->`
                Writer.Write("<!--"u8, key, "-->"u8);
                Writer.Write(value, format);
                Writer.Write("<!--/"u8, key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"{value:format}" {key}`
                Writer.Write("\""u8);
                Writer.Write(value, format);
                Writer.Write("\" "u8);
                Writer.Write(key);
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
        keyCursor.MoveNext();
        var key = keyCursor.Current;

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                // ex: `<!--{key}-->{value:format}<!--/{key}-->`
                Writer.Write("<!--"u8, key, "-->"u8);
                base.OnColorKeyhole(ref parent, value);
                Writer.Write("<!--/"u8, key, "-->"u8);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                // ex: `"{value:format}" {key}`
                Writer.Write("\""u8);
                base.OnColorKeyhole(ref parent, value, format);
                Writer.Write("\" "u8);
                Writer.Write(key);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                base.OnColorKeyhole(ref parent, value, format);
                break;
        }

        return true;
    }

    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null)
        => OnStringKeyhole(ref parent, value.ToString()); // TODO: Memory allocation!
        
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

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: false, format);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: true, format);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: false, format);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, includeEventArg: true, format);
    private bool OnListener(ref Html parent, bool includeEventArg, string? format = null)
    {
        if (deferredLiteral != null)
            HandleDeferredLiteral();

        keyCursor.MoveNext();
        var key = keyCursor.Current;

        if (includeEventArg)
        {
            // ex: `"keyholes.{key}.dispatchEvent(event.trim('{format ?? "*"}'))" {key}`
            Writer.Write("\"keyholes."u8);
            Writer.Write(key);
            Writer.Write(".dispatchEvent(event.trim('"u8);
            Writer.Write(format ?? "*");
            Writer.Write("'))\" "u8);
            Writer.Write(key);
        }
        else
        {
            // TODO: Is it better to use ({}) or () instead?
            // ex: `"keyholes.{key}.dispatchEvent(event.trim(''))" {key}`
            Writer.Write("\"keyholes."u8);
            Writer.Write(key);
            Writer.Write(".dispatchEvent(event.trim(''))\" "u8);
            Writer.Write(key);
        }
        
        attributeStatus = AttributeStatus.None;
        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current; // TODO: Remove?
        keyCursor.MoveDown();

        return true;
    }

    public override bool OnIterate<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        foreach (var html in enumerable)
        {
            htmls.AppendFormatted(html);
        }

        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        keyCursor.MoveUp();
        var key = keyCursor.Current;
        
        // Keyhole to represent the loop itself, useful for zero-length use cases.
        // ex: `<!--{key} /-->`
        Writer.Write("<!--"u8, key, " /-->"u8);

        return true;
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

        TryBeginAppend(literal.Length);
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
}