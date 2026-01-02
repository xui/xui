using System.Buffers;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public class XtmlComposer(IBufferWriter<byte> writer, WindowBuilder window) : HtmlComposer(writer)
{
    private StableKeyTreeWalker keyGenerator = new();

    public WindowBuilder Window { get; set; } = window;
    private enum AttributeStatus { None, Pending, InProgress }
    private AttributeStatus attributeStatus = AttributeStatus.None;
    private ReadOnlyMemory<char>? deferredLiteral = null;
    private bool isBodyOmitted = false;

    [ThreadStatic] static XtmlComposer? reusable;
    public static XtmlComposer Reuse(IBufferWriter<byte> writer, WindowBuilder window)
    {
        if (reusable is null)
            return reusable = new(writer, window);
        
        var composer = reusable;
        composer.Writer = writer;
        composer.Window = window;
        return composer;
    }

    public override void Reset()
    {
        Window = null!;
        attributeStatus = AttributeStatus.None;
        keyGenerator.Reset();
        base.Reset();
    }

    public override void OnTemplateBegin(ref Html html)
    {
        // TODO: Adjust keyGenerator so this step is not needed?  key:`key`
        html.Key = "key";
        keyGenerator.CreateNewGeneration(html.Key, html.Length);
        base.OnTemplateBegin(ref html);
    }

    public override bool OnTemplateEnd(ref Html html)
    {
        if (isBodyOmitted)
        {
            Encoding.UTF8.GetBytes("""
                    
                    </body>
                </html>
                """,
                Writer);
        }

        return base.OnTemplateEnd(ref html);
    }

    public override void OnElementBegin(ref Html html)
    {
        html.Key = keyGenerator.GetNextKey();

        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.WriteRaw($"<!--{html.Key}-->");
                break;
            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.WriteRaw($"\"");
                attributeStatus = AttributeStatus.InProgress;
                break;
        }

        keyGenerator.CreateNewGeneration(html.Key, html.Length);

        base.OnElementBegin(ref html);
    }

    public override bool OnElementEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.WriteRaw($"<!--/{html.Key}-->");

                if (format is {} transition)
                {
                    Writer.WriteRaw($$"""
                        <style>
                            ::view-transition-group(web4-fwd-{{html.Key}}, web4-rev-{{html.Key}}) { animation: none; }
                            ::view-transition-new(web4-fwd-{{html.Key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in; }
                            ::view-transition-old(web4-fwd-{{html.Key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out; }
                            ::view-transition-new(web4-rev-{{html.Key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out reverse; }
                            ::view-transition-old(web4-rev-{{html.Key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in reverse; }
                        </style>
                        """);
                }
                break;

            case AttributeStatus.InProgress:
                Writer.WriteRaw($"""
                    " {html.Key}
                    """);
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.Pending:
                throw new NotSupportedException("Attributes cannot have nested Htmls");
        }

        var cursor = parent.Type != HtmlType.Enumeration ? parent.Cursor : parent.Cursor * 2;
        keyGenerator.ReturnToParent(parent.Key, cursor, parent.Length);

        return base.OnElementEnd(ref parent, html, format, expression);
    }

    public override bool OnMarkup(ref Html parent, string literal)
    {
        int offset = IsBeforeAppend ? InjectBootloader(literal) : 0;

        // This makes the assumption that keyholes preceeded with an '=' are 
        // always attributes.  Attributes need different sentinels than regular
        // keyholes and boolean attributes have a few strange rules to follow:
        // https://developer.mozilla.org/en-US/docs/Glossary/Boolean/HTML
        if (literal.EndsWith('='))
        {
            attributeStatus = AttributeStatus.Pending;
            deferredLiteral = literal.AsMemory(offset);
        }
        else
        {
            Encoding.UTF8.GetBytes(literal.AsSpan(offset), Writer);
        }

        return CompleteStringLiteral(literal.Length);
    }

    public override bool OnStringKeyhole(ref Html parent, string value)
    {
        // Strings have no format strings.

        var key = keyGenerator.GetNextKey();
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.WriteRaw($"""
                    <!--{key}-->{value}<!--/{key}-->
                    """);
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.WriteRaw($"""
                    "{value}" {key}
                    """);
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                return base.OnStringKeyhole(ref parent, value);
        }

        return CompleteFormattedValue();
    }

    public override bool OnBoolKeyhole(ref Html parent, bool value)
    {
        var key = keyGenerator.GetNextKey();
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                var b = value ? "true" : "false";
                Writer.WriteRaw($"""
                    <!--{key}-->{b}<!--/{key}-->
                    """);
                break;

            case AttributeStatus.Pending:
                var attributeName = HandleDeferredLiteral(isBooleanAttribute: true);
                if (value)
                {
                    Writer.WriteRaw($"""
                         {attributeName} {key}="{attributeName}"
                        """);
                }
                else
                {
                    Writer.WriteRaw($"""
                         {key}="{attributeName}"
                        """);
                }
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                return base.OnBoolKeyhole(ref parent, value);
        }

        return CompleteFormattedValue();
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
    public override bool OnUtf8SpanFormattable<T>(ref Html parent, T value, string? format = null)
        // where T : struct, IUtf8SpanFormattable // (inherited)
    {
        // Wraps the mutable value with two comment tags
        // to separate it from any neighboring text.
        // At the end of the body an inline script registers them 
        // because we can't rely on id= or document.getElementById().
        // It should end up looking like this:
        // $"<!--key123-->{value:format}<!--/key123-->"

        var key = keyGenerator.GetNextKey();
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.WriteRaw($"<!--{key}-->");
                base.OnUtf8SpanFormattable(ref parent, value, format);
                Writer.WriteRaw($"<!--/{key}-->");
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.WriteRaw($"\"");
                base.OnUtf8SpanFormattable(ref parent, value, format);
                Writer.WriteRaw($"\" {key}");
                // status jumps from .Pending to .None because the whole 
                // attribute is just one value, not a bunch of keyholes+literals.
                attributeStatus = AttributeStatus.None;
                break;

            case AttributeStatus.InProgress:
                // No sentinels.  This keyhole is a part of a larger attribute
                // composed of multiple keyholes+literals.  Write only the value.
                base.OnUtf8SpanFormattable(ref parent, value, format);
                break;
        }

        return true;
    }

    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null)
    {
        var key = keyGenerator.GetNextKey();
        switch (attributeStatus)
        {
            case AttributeStatus.None:
                Writer.WriteRaw($"<!--{key}-->");
                base.OnColorKeyhole(ref parent, value, format);
                Writer.WriteRaw($"<!--/{key}-->");
                break;

            case AttributeStatus.Pending:
                HandleDeferredLiteral();
                Writer.WriteRaw($"\"");
                base.OnColorKeyhole(ref parent, value, format);
                Writer.WriteRaw($"\" {key}");
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

        Encoding.UTF8.GetBytes(deferredLiteral.Value.Span, Writer);
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

        Encoding.UTF8.GetBytes(deferredLiteralSpan[..indexBeforeAttribute], Writer);
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

        var key = keyGenerator.GetNextKey();
        if (includeEventArg)
        {
            Writer.WriteRaw($"""
                "keyholes.{key}.dispatchEvent(event.trim('{format ?? "*"}'))" {key}
                """);
        }
        else
        {
            // TODO: Is it better to use ({}) or () instead?
            Writer.WriteRaw($"""
                "keyholes.{key}.dispatchEvent(event.trim(''))" {key}
                """);
        }
        
        attributeStatus = AttributeStatus.None;
        return CompleteFormattedValue();
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        htmls.Key = keyGenerator.GetNextKey();
        keyGenerator.CreateNewGeneration(htmls.Key, htmls.Length);
        return true;
    }

    public override bool OnIterate<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        foreach (var partial in enumerable)
        {
            htmls.AppendFormatted(partial);
        }

        return CompleteFormattedValue();
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        // Keyhole to represent the loop itself, useful for zero-length use cases.
        Writer.WriteRaw($"<!--{htmls.Key} /-->");

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return true;
    }

    private static readonly string BOOTLOADER = 
        new StreamReader(System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("Web4.Bootloader.html")!
        )
        .ReadToEnd();

    private int InjectBootloader(string literal)
    {
        // Wait for the next append where there's actual content.
        if (literal.Length == 0)
            return 0;

        Writer.WriteRaw($"""
            <!doctype html>
            <html>
            <head>
            
            """);

        // Write dev-included <head> content (if any)
        int headStart = literal.IndexOf("<head>");
        bool isHeadOmitted = headStart < 0;
        if (!isHeadOmitted)
        {
            headStart += 6; // "<head>".Length;
            int headEnd = literal.IndexOf("</head>");
            if (headEnd > headStart)
            {
                Encoding.UTF8.GetBytes(
                    chars: literal.AsSpan(headStart..headEnd),
                    writer: Writer
                );
            }
        }

        // Write necesary JavaScript and CSS to operate Web4
        Encoding.UTF8.GetBytes(BOOTLOADER, Writer);

        // Write event handlers set on window or document
        if (Window.Listeners.Count > 0)
        {
            Encoding.UTF8.GetBytes("\n\n<script>\n", Writer);

            foreach (var listener in Window.Listeners)
                Writer.WriteRaw($"  {listener.Html}\n");

            Encoding.UTF8.GetBytes("</script>\n\n", Writer);
        }

        // Locate the start of the <body> tag (if present)
        int bodyStart = literal.IndexOf("<body");
        this.isBodyOmitted = bodyStart < 0;
        if (isBodyOmitted)
        {
            Encoding.UTF8.GetBytes("""

                </head>
                <body>
                
                """, Writer);
            return 0;
        }
        else
        {
            Encoding.UTF8.GetBytes("""
                
                </head>

                """, Writer);
            return bodyStart;
        }
    }
}