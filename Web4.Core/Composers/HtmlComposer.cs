using System.Buffers;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public class HtmlComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    [ThreadStatic] static HtmlComposer? reusable;
    public static HtmlComposer Reuse(IBufferWriter<byte> writer)
    {
        if (reusable is null)
            return reusable = new(writer);
        
        var composer = reusable;
        composer.Writer = writer;
        return composer;
    }

    public override void Reset()
    {
        Writer = null!;
        base.Reset();
    }

    public override bool OnMarkup(ref Html parent, string literal)
    {
        var destination = Writer.GetSpan(literal.Length);
        var length = Encoding.UTF8.GetBytes(literal, destination);
        Writer.Advance(length);

        return base.OnMarkup(ref parent, literal);
    }

    public override bool OnString(ref Html parent, string value)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        var destination = Writer.GetSpan(value.Length);
        var length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        return base.OnString(ref parent, value);
    }

    public override bool OnBool(ref Html parent, bool value)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var output = value ? "true" : "false";
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.OnBool(ref parent, value);
    }

    public override bool OnInt(ref Html parent, int value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnLong(ref Html parent, long value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnFloat(ref Html parent, float value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDouble(ref Html parent, double value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDecimal(ref Html parent, decimal value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDateTime(ref Html parent, DateTime value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDateOnly(ref Html parent, DateOnly value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnTimeSpan(ref Html parent, TimeSpan value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnTimeOnly(ref Html parent, TimeOnly value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public virtual bool OnUtf8SpanFormattable<T>(ref Html parent, T value, string? format = null)
        where T : struct, IUtf8SpanFormattable
    {
        var destination = Writer.GetSpan(128);  // TODO: Research the true max length of T.
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        return CompleteFormattedValue();
    }

    public override bool OnColor(ref Html parent, Color value, string? format = null)
    {
        var destination = Writer.GetSpan(value.GetMaxPossibleLength());
        value.TryFormat(destination, out int length, format);
        Writer.Advance(length);

        return base.OnColor(ref parent, value);
    }

    public override bool OnUri(ref Html parent, Uri value, string? format = null)
    {
        // TODO: Fix memory allocation happening here and incorporate format strings
        var output = value.ToString();
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.OnUri(ref parent, value);
    }


    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    
    private bool HandleNotSupported()
    {
        // attributeName is already written at the end of the prior string literal (e.g. <button onclick=)
        Encoding.UTF8.GetBytes("\"\"", Writer);
        return CompleteFormattedValue();
    }
}