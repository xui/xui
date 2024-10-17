using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public class DefaultComposer(IBufferWriter<byte> writer) : StreamingComposer(writer)
{
    public override bool AppendLiteral(string s)
    {
        Span<byte> destination = Writer.GetSpan(s.Length);
        int length = Encoding.UTF8.GetBytes(s, destination);
        Writer.Advance(length);

        return base.AppendLiteral(s);
    }

    public override bool AppendFormatted(string s)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        Span<byte> destination = Writer.GetSpan(s.Length);
        int length = Encoding.UTF8.GetBytes(s, destination);
        Writer.Advance(length);

        return base.AppendFormatted(s);
    }

    public override bool AppendFormatted(int i, string? format = null)
    {
        AppendUtf8SpanFormattable(i, format);

        return base.AppendFormatted(i, format);
    }

    public override bool AppendFormatted(long l, string? format = null)
    {
        AppendUtf8SpanFormattable(l, format);

        return base.AppendFormatted(l, format);
    }

    public override bool AppendFormatted(float f, string? format = null)
    {
        AppendUtf8SpanFormattable(f, format);

        return base.AppendFormatted(f, format);
    }

    public override bool AppendFormatted(double d, string? format = null)
    {
        AppendUtf8SpanFormattable(d, format);

        return base.AppendFormatted(d, format);
    }

    public override bool AppendFormatted(decimal d, string? format = null)
    {
        AppendUtf8SpanFormattable(d, format);

        return base.AppendFormatted(d, format);
    }

    public override bool AppendFormatted(DateTime d, string? format = null)
    {
        AppendUtf8SpanFormattable(d, format);

        return base.AppendFormatted(d, format);
    }

    public override bool AppendFormatted(TimeSpan t, string? format = null)
    {
        AppendUtf8SpanFormattable(t, format);

        return base.AppendFormatted(t, format);
    }

    private void AppendUtf8SpanFormattable<T>(T value, ReadOnlySpan<char> format = default) where T : IUtf8SpanFormattable
    {
        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);
    }

    public override bool AppendFormatted(bool b)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var value = b ? Boolean.TrueString : Boolean.FalseString;
        Span<byte> destination = Writer.GetSpan(value.Length);
        int length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        return base.AppendFormatted(b);
    }

    public override bool AppendFormatted(Func<Event, Html> attribute, string? expression = null)
    {
        var name = GetAttributeName(expression);
        Writer.WriteRaw($"{name}=\"");
        attribute(Event.Empty);
        Writer.WriteRaw($"\"");

        return base.AppendFormatted(attribute, expression);
    }

    public override bool AppendFormatted<T>(Func<Event, T> attribute, string? expression = null)
    {
        var name = GetAttributeName(expression);
        var value = attribute(Event.Empty);
        Writer.WriteRaw($"{name}=\"{value}\"");

        return base.AppendFormatted(attribute, expression);
    }

    public override bool AppendFormatted(Func<Event, bool> attribute, string? expression = null)
    {
        var value = attribute(Event.Empty);

        if (value)
        {
            var name = GetAttributeName(expression);
            Writer.WriteRaw($"{name}");
        }

        return base.AppendFormatted(attribute, expression);
    }

    public override bool AppendFormatted(Action eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler);
    }

    public override bool AppendFormatted(Action<Event> eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler);
    }

    public override bool AppendFormatted(Func<Task> eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler);
    }

    public override bool AppendFormatted(Func<Event, Task> eventHandler, string? expression = null)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        return base.AppendFormatted(eventHandler);
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

    public override bool AppendFormatted<TView>(TView view) => AppendFormatted(view.Render());
    public override bool AppendFormatted(Slot slot) => AppendFormatted(slot());

    public override bool AppendFormatted(Html partial)
    {
        // Nothing to write.
        // It was already written as "partial" (above) was being created.
        
        return base.AppendFormatted(partial);
    }

    public override bool AppendFormatted(RawText text)
    {
        // Nothing to write.
        // It was already written as "text" (above) was being created.
        
        return base.AppendFormatted(text);
    }
}