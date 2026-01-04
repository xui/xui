using System.Buffers;
using System.Drawing;

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

    public override bool OnMarkup(ref Html parent, string literal) => Writer.WriteUtf8(literal);
    public override bool OnStringKeyhole(ref Html parent, string value) => Writer.WriteUtf8(value);
    public override bool OnBoolKeyhole(ref Html parent, bool value) => Writer.WriteUtf8(value ? "true" : "false");
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => Writer.WriteUtf8(value, format);
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => Writer.WriteUtf8(value.ToString());
    // TODO: ^ Fix memory allocation happening here and incorporate format strings

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    private bool HandleNotSupported() => Writer.WriteUtf8("\"\""); // attributeName is already written at the end of the prior string literal (e.g. <button onclick=)
}