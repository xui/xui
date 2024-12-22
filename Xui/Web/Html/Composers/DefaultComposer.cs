using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class DefaultComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool WriteImmutableMarkup(int index, string literal)
    {
        var destination = Writer.GetSpan(literal.Length);
        var length = Encoding.UTF8.GetBytes(literal, destination);
        Writer.Advance(length);

        return base.WriteImmutableMarkup(index, literal);
    }

    public override bool WriteMutableValue(int index, string value)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        var destination = Writer.GetSpan(value.Length);
        var length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        return base.WriteMutableValue(index, value);
    }

    public override bool WriteMutableValue(int index, bool value)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var output = value ? Boolean.TrueString : Boolean.FalseString;
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.WriteMutableValue(index, value);
    }

    public override bool WriteMutableValue<T>(int index, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        return base.WriteMutableValue(index, value, format);
    }

    public override bool WriteMutableAttribute(int index, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        // Boolean attributes are interesting in that the DOM treats them
        // as true regardless of what value you supply.  The only way to 
        // evaluate a boolean attribute to false is to exclude it.

        var isTrue = attrValue(Event.Empty);
        if (isTrue)
        {
            Encoding.UTF8.GetBytes(attrName, Writer);
            // Boolean attributes don't need any value.
        }

        return base.WriteMutableAttribute(index, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(int index, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        var value = attrValue(Event.Empty);
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.WriteMutableAttribute(index, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(int index, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler)
        attrValue(string.Empty);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.WriteMutableAttribute(index, attrName, attrValue, expression);
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
        // attributeName is already written at the end of the prior string literal (e.g. <button onclick=)
        Encoding.UTF8.GetBytes("\"\"", Writer);
        return CompleteFormattedValue();
    }

    private bool HandleNotSupported(ReadOnlySpan<char> attributeName)
    {
        Encoding.UTF8.GetBytes(attributeName, Writer);
        Encoding.UTF8.GetBytes("=\"\"", Writer);
        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(int index, TView view) => WriteMutableElement(index, view.Render());
    public override bool WriteMutableElement(int index, Slot slot) => WriteMutableElement(index, slot());

    public override bool WriteMutableElement(int index, Html partial, string? expression = null)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler 
        // https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        
        return base.WriteMutableElement(index, partial, expression);
    }
}