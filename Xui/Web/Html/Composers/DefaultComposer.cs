using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class DefaultComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool AppendLiteral(string literal)
    {
        Span<byte> destination = Writer.GetSpan(literal.Length);
        int length = Encoding.UTF8.GetBytes(literal, destination);
        Writer.Advance(length);

        return base.AppendLiteral(literal);
    }

    public override bool AppendFormatted(string value)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        Span<byte> destination = Writer.GetSpan(value.Length);
        int length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        return base.AppendFormatted(value);
    }

    public override bool AppendFormatted<T>(T value, string? format = default)
    // where T : struct, IUtf8SpanFormattable // (from base)
    {
        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        return base.AppendFormatted(value, format);
    }

    public override bool AppendFormatted(bool value)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var output = value ? Boolean.TrueString : Boolean.FalseString;
        Span<byte> destination = Writer.GetSpan(output.Length);
        int length = Encoding.UTF8.GetBytes(output, destination);
        Writer.Advance(length);

        return base.AppendFormatted(value);
    }

    public override bool AppendFormatted(Func<Event, Html> attribute, string? expression = null)
    {
        var name = GetAttributeName(expression);

        Encoding.UTF8.GetBytes(name, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler)
        attribute(Event.Empty);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.AppendFormatted(attribute, expression);
    }

    public override bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (IsReservedForEvent(name) || IsReservedForEventHandler(name))
        {
            return base.AppendFormatted(attribute, expression);
        }

        var value = attribute(Event.Empty);

        Encoding.UTF8.GetBytes(name, Writer);
        Encoding.UTF8.GetBytes("=\"", Writer);

        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        Encoding.UTF8.GetBytes("\"", Writer);

        return base.AppendFormatted(attribute, expression);
    }

    public override bool AppendFormatted(Func<Event, bool> attribute, string? expression = null)
    {
        var name = GetAttributeName(expression);
        if (IsReservedForEvent(name) || IsReservedForEventHandler(name))
        {
            return base.AppendFormatted(attribute, expression);
        }

        var value = attribute(Event.Empty);

        if (value)
        {
            Encoding.UTF8.GetBytes(name, Writer);
        }

        return base.AppendFormatted(attribute, expression);
    }

    public override bool AppendFormatted(Action eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler, expression);
    }

    public override bool AppendFormatted(Action<Event> eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler, expression);
    }

    public override bool AppendFormatted(Func<Task> eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler, expression);
    }

    public override bool AppendFormatted(Func<Event, Task> eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler, expression);
    }

    public override bool AppendFormatted<TView>(TView view) => AppendFormatted(view.Render());
    public override bool AppendFormatted(Slot slot) => AppendFormatted(slot());

    public override bool AppendFormatted(Html partial)
    {
        // Instantiating an Html object causes its contents to be 
        // written to the stream due to the compiler's lowered code.
        // (see: InterpolatedStringHandler)
        
        return base.AppendFormatted(partial);
    }

    protected static ReadOnlySpan<char> GetAttributeName(string? expression)
    {
        if (expression is not null && expression.Length >= 2)
        {
            // TODO: Make sure this doesn't allocate.
            var end = expression.IndexOfAny([' ', '=']);
            if (end > 0)
            {
                var start = expression[0] == '@' ? 1 : 0;
                return expression.AsSpan(start, end - start);
            }
        }
        return "attribute-name-unspecified";
    }

    // We have an unfortunate edge case to handle here.  
    // The notation used for some attributes:
    //   $"<input type="text" { maxlength => c } />"
    // technically also matches the signature used for events:
    //   $"<button { onclick => c++ }>click me</button>"
    // Fortunately there's a simple workaround.  Since attributes
    // only use the input param for its name, never its value 
    // we can just key off its name and send it down a different path
    // as if it were an event handler.
    protected static bool IsReservedForEvent(ReadOnlySpan<char> name) => 
        name switch
        {
            "e" or
            "ev" or
            "evnt" or
            "@event" or
            "(e)" or
            "(ev)" or
            "(evnt)" or
            "(@event)" => true,
            _ => false
        };

    protected static bool IsReservedForEventHandler(ReadOnlySpan<char> name) => 
        // All events start with on*.
        name.Length >= 2 && name[0] == 'o' && name[1] == 'n';
}