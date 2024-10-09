using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web;

public class DefaultComposer : Composer
{
    public DefaultComposer(IBufferWriter<byte> writer)
        : base(writer)
    {
    }

    public override void AppendLiteral(string s)
    {
        Span<byte> destination = Writer.GetSpan(s.Length);
        int length = Encoding.UTF8.GetBytes(s, destination);
        Writer.Advance(length);

        base.AppendLiteral(s);
    }

    public override void AppendFormatted(string s)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        Span<byte> destination = Writer.GetSpan(s.Length);
        int length = Encoding.UTF8.GetBytes(s, destination);
        Writer.Advance(length);

        base.AppendFormatted(s);
    }

    public override void AppendFormatted(int i, string? format = null)
    {
        AppendUtf8SpanFormattable(i, format);

        base.AppendFormatted(i, format);
    }

    public override void AppendFormatted(long l, string? format = null)
    {
        AppendUtf8SpanFormattable(l, format);

        base.AppendFormatted(l, format);
    }

    public override void AppendFormatted(float f, string? format = null)
    {
        AppendUtf8SpanFormattable(f, format);

        base.AppendFormatted(f, format);
    }

    public override void AppendFormatted(double d, string? format = null)
    {
        AppendUtf8SpanFormattable(d, format);

        base.AppendFormatted(d, format);
    }

    public override void AppendFormatted(decimal d, string? format = null)
    {
        AppendUtf8SpanFormattable(d, format);

        base.AppendFormatted(d, format);
    }

    public override void AppendFormatted(DateTime d, string? format = null)
    {
        AppendUtf8SpanFormattable(d, format);

        base.AppendFormatted(d, format);
    }

    public override void AppendFormatted(TimeSpan t, string? format = null)
    {
        AppendUtf8SpanFormattable(t, format);

        base.AppendFormatted(t, format);
    }

    private void AppendUtf8SpanFormattable<T>(T value, ReadOnlySpan<char> format = default) where T : IUtf8SpanFormattable
    {
        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);
    }

    public override void AppendFormatted(bool b)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var value = b ? Boolean.TrueString : Boolean.FalseString;
        Span<byte> destination = Writer.GetSpan(value.Length);
        int length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        base.AppendFormatted(b);
    }

    public override void AppendFormatted<TView>(TView v) => AppendFormatted(v.Render());
    public override  void AppendFormatted(Slot s) => AppendFormatted(s());

    public override void AppendFormatted(Html h)
    {
        // Nothing to write.
        // It was already written as "h" (above) was being created.
        
        base.AppendFormatted(h);
    }

    public override void AppendFormatted(Action a)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        base.AppendFormatted(a);
    }

    public override void AppendFormatted(Action<Event> a)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        base.AppendFormatted(a);
    }

    public override void AppendFormatted(Func<Task> f)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        base.AppendFormatted(f);
    }

    public override void AppendFormatted(Func<Event, Task> f)
    {
        // Nothing to write.  Possibly throw if SSG?
        
        base.AppendFormatted(f);
    }
}