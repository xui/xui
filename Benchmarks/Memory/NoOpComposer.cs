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

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null) => true;
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null) => true;
    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null) => true; // where T : struct, IUtf8SpanFormattable
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null) => true;
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Color> attrValue, string? expression = null) => true;
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Uri> attrValue, string? expression = null) => true;

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventListener(ref Html parent, ReadOnlySpan<char> argName, Action<object> listener, string? expression = null) => HandleNotSupported();

    private bool HandleNotSupported() => true;
    private bool HandleNotSupported(ReadOnlySpan<char> attributeName) => true;

    public override bool WriteMutableElement<TComponent>(ref Html parent, ref TComponent component, string? format = null, string? expression = null) => OnHtmlPartialEnds(ref parent, component.Render(), format, expression);
    public override bool OnHtmlPartialEnds(ref Html parent, Html partial, string? format = null, string? expression = null) => true;
}