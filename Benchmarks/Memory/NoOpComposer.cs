using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public class NoOpComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(ref Html parent, string literal) => true;

    public override bool WriteMutableValue(ref Html parent, string value) => true;
    public override bool WriteMutableValue(ref Html parent, bool value) => true;
    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null) => true;
    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => true;
    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null) => true; // where T : struct, IUtf8SpanFormattable // (from base)

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => true;
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => true;
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => true;
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => true;

    public override bool OnHtmlPartialEnds(ref Html parent, ref Html partial, string? format = null, string? expression = null) => true;
}