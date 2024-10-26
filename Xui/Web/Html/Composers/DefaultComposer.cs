using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class DefaultComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool AppendStaticPartialMarkup(string literal)
    {
        var destination = Writer.GetSpan(literal.Length);
        var length = Encoding.UTF8.GetBytes(literal, destination);
        Writer.Advance(length);

        return base.AppendStaticPartialMarkup(literal);
    }

    public override bool AppendDynamicValue(string value)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        var destination = Writer.GetSpan(value.Length);
        var length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        return base.AppendDynamicValue(value);
    }

    public override bool AppendDynamicValue(bool value)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var output = value ? Boolean.TrueString : Boolean.FalseString;
        var destination = Writer.GetSpan(output.Length);
        var length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.AppendDynamicValue(value);
    }

    public override bool AppendDynamicValue<T>(T value, string? format = default)
        // where T : IUtf8SpanFormattable // (from base)
    {
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        return base.AppendDynamicValue(value, format);
    }

    public override bool AppendDynamicAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue)
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

        return base.AppendDynamicAttribute(attrName, attrValue);
    }

    public override bool AppendDynamicAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null)
        // where T : IUtf8SpanFormattable
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        var value = attrValue(Event.Empty);
        var destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.AppendDynamicAttribute(attrName, attrValue, format);
    }

    public override bool AppendDynamicAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue)
    {
        Encoding.UTF8.GetBytes(attrName, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler)
        attrValue(string.Empty);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.AppendDynamicAttribute(attrName, attrValue);
    }

    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Action eventHandler) => HandleNotSupported(argName);
    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Action<Event> eventHandler) => HandleNotSupported(argName);
    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Func<Task> eventHandler) => HandleNotSupported(argName);
    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Func<Event, Task> eventHandler) => HandleNotSupported(argName);
    
    private bool HandleNotSupported(ReadOnlySpan<char> argName)
    {
        if (!argName.IsEmpty)
        {
            Encoding.UTF8.GetBytes(argName, Writer);
            Encoding.UTF8.GetBytes("""
                =""
                """, Writer);
        }
        else
        {
            Encoding.UTF8.GetBytes("""
                ""
                """, Writer);
        }

        return CompleteDynamic(1);
    }

    public override bool AppendDynamicElement<TView>(TView view) => AppendDynamicElement(view.Render());
    public override bool AppendDynamicElement(Slot slot) => AppendDynamicElement(slot());

    public override bool AppendDynamicElement(Html partial)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler 
        // https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        
        return base.AppendDynamicElement(partial);
    }
}