using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Web4.Composers;

public class NoOpComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        return true;
    }

    public override bool WriteMutableValue(ref Html parent, string value)
    {
        return true;
    }

    public override bool WriteMutableValue(ref Html parent, bool value)
    {
        return true;
    }

    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        return true;
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        return true;
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteEventHandler(ref Html parent, Action listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => HandleNotSupported();
    
    private bool HandleNotSupported()
    {
        return true;
    }

    private bool HandleNotSupported(ReadOnlySpan<char> attributeName)
    {
        return true;
    }

    public override bool WriteMutableElement<TView>(ref Html parent, TView view) => WriteMutableElement(ref parent, view.Render());
    public override bool WriteMutableElement(ref Html parent, Slot slot) => WriteMutableElement(ref parent, slot());

    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null)
    {
        return true;
    }
}