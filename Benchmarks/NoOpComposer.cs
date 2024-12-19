using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class NoOpComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool AppendImmutableMarkup(string literal)
    {
        return true;
    }

    public override bool AppendMutableValue(string value)
    {
        return true;
    }

    public override bool AppendMutableValue(bool value)
    {
        return true;
    }

    public override bool AppendMutableValue<T>(T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        return true;
    }

    public override bool AppendMutableAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool AppendMutableAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        return true;
    }

    public override bool AppendMutableAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        return true;
    }

    public override bool AppendEventHandler(Action eventHandler, string? expression = null) => HandleNotSupported();
    public override bool AppendEventHandler(Action<Event> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool AppendEventHandler(Func<Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool AppendEventHandler(Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported();
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    public override bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => HandleNotSupported(attributeName);
    
    private bool HandleNotSupported()
    {
        return true;
    }

    private bool HandleNotSupported(ReadOnlySpan<char> attributeName)
    {
        return true;
    }

    public override bool AppendMutableElement<TView>(TView view) => AppendMutableElement(view.Render());
    public override bool AppendMutableElement(Slot slot) => AppendMutableElement(slot());

    public override bool AppendMutableElement(Html partial, string? expression = null)
    {
        return true;
    }
}