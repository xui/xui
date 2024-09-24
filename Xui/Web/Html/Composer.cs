using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web;

public class Composer
{
    [ThreadStatic]
    static Composer? current;
    public static Composer? Current { get => current; set => current = value; }
    public IBufferWriter<byte> Writer { get; set; }

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    public Composer(IBufferWriter<byte> writer)
    {
        this.Writer = writer;
    }

    public void GrowStatic(int literalLength)
    {
        literalLengthRemaining += literalLength;
    }

    public void GrowDynamic(int formattedCount)
    {
        formattedValuesRemaining += formattedCount;
    }

    private void CompleteStatic(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        MoveNext();
    }

    private void CompleteDynamic(int formattedCount)
    {
        formattedValuesRemaining -= formattedCount;
        MoveNext();
    }

    private void MoveNext()
    {
        if (literalLengthRemaining == 0 && formattedValuesRemaining == 0)
        {
            Clear();
        }
    }

    private void Clear()
    {
        current = null;
    }

    public void AppendLiteral(string s)
    {
        Span<byte> destination = Writer.GetSpan(s.Length);
        int length = Encoding.UTF8.GetBytes(s, destination);
        Writer.Advance(length);

        CompleteStatic(s.Length);
    }

    public void AppendFormatted(string s)
    {
        // string has no formatters (and alignment isn't helpful in HTML)
        Span<byte> destination = Writer.GetSpan(s.Length);
        int length = Encoding.UTF8.GetBytes(s, destination);
        Writer.Advance(length);

        CompleteDynamic(1);
    }

    public void AppendFormatted<T>(T value, ReadOnlySpan<char> format = default) where T : IUtf8SpanFormattable
    {
        Span<byte> destination = Writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        Writer.Advance(length);

        CompleteDynamic(1);
    }

    public void AppendFormatted(bool b)
    {
        // bool has no formatters and doesn't implement IUtf8SpanFormattable
        var value = b ? System.Boolean.TrueString : System.Boolean.FalseString;
        Span<byte> destination = Writer.GetSpan(value.Length);
        int length = Encoding.UTF8.GetBytes(value, destination);
        Writer.Advance(length);

        CompleteDynamic(1);
    }

    public void AppendFormatted<TView>(TView v) where TView : IView
    {
        AppendFormatted(v.Render());
    }

    public void AppendFormatted(Html h)
    {
        // Nothing to write.
        // It was already written as "h" (above) was being created.
        CompleteDynamic(1);
    }

    public void AppendFormatted(Slot s)
    {
        // Calls AppendFormatted(Html h).
        AppendFormatted(s());
    }

    public void AppendFormatted(Action a)
    {
        // Nothing to write.  Possibly throw if SSG?
        CompleteDynamic(1);
    }

    public void AppendFormatted(Action<Event> a)
    {
        // Nothing to write.  Possibly throw if SSG?
        CompleteDynamic(1);
    }

    public void AppendFormatted(Func<Task> f)
    {
        // Nothing to write.  Possibly throw if SSG?
        CompleteDynamic(1);
    }

    public void AppendFormatted(Func<Event, Task> f)
    {
        // Nothing to write.  Possibly throw if SSG?
        CompleteDynamic(1);
    }
}