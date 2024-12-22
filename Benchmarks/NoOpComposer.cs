using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class NoOpComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(int index, string literal)
    {
        return true;
    }

    public override bool WriteMutableValue(int index, string value)
    {
        return true;
    }

    public override bool WriteMutableValue(int index, bool value)
    {
        return true;
    }

    public override bool WriteMutableValue<T>(int index, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        return true;
    }

    public override bool WriteMutableAttribute(int index, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteMutableAttribute<T>(int index, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        return true;
    }

    public override bool WriteMutableAttribute(int index, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool WriteEventHandler(int index, Action eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(int index, Action<Event> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(int index, Func<Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(int index, Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    
    private bool HandleNotSupported()
    {
        return true;
    }

    private bool HandleNotSupported(ReadOnlySpan<char> attributeName)
    {
        return true;
    }

    public override bool WriteMutableElement<TView>(int index, TView view) => WriteMutableElement(index, view.Render());
    public override bool WriteMutableElement(int index, Slot slot) => WriteMutableElement(index, slot());

    public override bool WriteMutableElement(int index, Html partial, string? expression = null)
    {
        return true;
    }
}