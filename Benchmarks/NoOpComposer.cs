using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class NoOpComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(string literal)
    {
        return true;
    }

    public override bool WriteMutableValue(string value)
    {
        return true;
    }

    public override bool WriteMutableValue(bool value)
    {
        return true;
    }

    public override bool WriteMutableValue<T>(T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        return true;
    }

    public override bool WriteMutableAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteMutableAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        return true;
    }

    public override bool WriteMutableAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteEventHandler(Action eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(Action<Event> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(Func<Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    
    private bool HandleNotSupported()
    {
        return true;
    }

    private bool HandleNotSupported(ReadOnlySpan<char> attributeName)
    {
        return true;
    }

    public override bool WriteMutableElement<TView>(TView view) => WriteMutableElement(view.Render());
    public override bool WriteMutableElement(Slot slot) => WriteMutableElement(slot());

    public override bool WriteMutableElement(Html partial, string? expression = null)
    {
        return true;
    }
}