using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class NoOpComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(ref Html html, string literal)
    {
        return true;
    }

    public override bool WriteMutableValue(ref Html html, string value)
    {
        return true;
    }

    public override bool WriteMutableValue(ref Html html, bool value)
    {
        return true;
    }

    public override bool WriteMutableValue<T>(ref Html html, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        return true;
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteMutableAttribute<T>(ref Html html, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        return true;
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteEventHandler(ref Html html, Action eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html html, Action<Event> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html html, Func<Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html html, Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    
    private bool HandleNotSupported()
    {
        return true;
    }

    private bool HandleNotSupported(ReadOnlySpan<char> attributeName)
    {
        return true;
    }

    public override bool WriteMutableElement<TView>(ref Html html, TView view) => WriteMutableElement(ref html, view.Render());
    public override bool WriteMutableElement(ref Html html, Slot slot) => WriteMutableElement(ref html, slot());

    public override bool WriteMutableElement(ref Html html, Html partial, string? expression = null)
    {
        return true;
    }
}