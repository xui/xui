using System.Buffers;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public class HtmlComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        var destination = Writer.GetSpan(literal.Length);
        var length = Encoding.UTF8.GetBytes(literal, destination);
        Writer.Advance(length);

        return base.WriteImmutableMarkup(ref parent, literal);
    }

    public override bool WriteMutableValue(ref Html parent, string value)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        var destination = Writer.GetSpan(value.Length);
        var length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, bool value)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var output = value ? "true" : "false";
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null)
    {
        var destination = Writer.GetSpan(value.GetMaxPossibleLength());
        value.TryFormat(destination, out int length, format);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null)
    {
        // TODO: Fix memory allocation happening here
        var output = value.ToString();
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value, format);
    }


    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    
    private bool HandleNotSupported()
    {
        // attributeName is already written at the end of the prior string literal (e.g. <button onclick=)
        Encoding.UTF8.GetBytes("\"\"", Writer);
        return CompleteFormattedValue();
    }
}