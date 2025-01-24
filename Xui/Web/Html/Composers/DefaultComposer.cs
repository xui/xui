using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Web4.Composers;

public class DefaultComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
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
        var output = value ? Boolean.TrueString : Boolean.FalseString;
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        return base.WriteMutableValue(ref parent, value, format);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null)
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        var value = attrValue(null!);
        Encoding.UTF8.GetBytes(value, Writer);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        // Boolean attributes are interesting in that the DOM treats them
        // as true regardless of what value you supply.  The only way to 
        // evaluate a boolean attribute to false is to exclude it.

        var isTrue = attrValue(null!);
        if (isTrue)
        {
            Encoding.UTF8.GetBytes(attrName, Writer);
            // Boolean attributes don't need any value.
        }

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        var value = attrValue(null!);
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null)
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler)
        attrValue(null!);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteEventHandler(ref Html parent, Action eventHandler, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html parent, Action<Event> eventHandler, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html parent, Func<Task> eventHandler, string? format = null, string? expression = null) => HandleNotSupported();
    public override bool WriteEventHandler(ref Html parent, Func<Event, Task> eventHandler, string? format = null, string? expression = null) => HandleNotSupported();
    
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

    public override bool WriteMutableElement<TView>(ref Html parent, TView view) => WriteMutableElement(ref parent, view.Render());
    public override bool WriteMutableElement(ref Html parent, Slot slot) => WriteMutableElement(ref parent, slot());

    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler 
        // https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        
        return base.WriteMutableElement(ref parent, partial, expression);
    }
}