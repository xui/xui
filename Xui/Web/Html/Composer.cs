using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web;

public abstract class Composer
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

    protected void CompleteStatic(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        MoveNext();
    }

    protected void CompleteDynamic(int formattedCount)
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

    public abstract void AppendLiteral(string s);
    public abstract void AppendFormatted(string s);
    public abstract void AppendFormatted(int i, string? format = null);
    public abstract void AppendFormatted(long l, string? format = null);
    public abstract void AppendFormatted(float f, string? format = null);
    public abstract void AppendFormatted(double d, string? format = null);
    public abstract void AppendFormatted(decimal d, string? format = null);
    public abstract void AppendFormatted(DateTime d, string? format = null);
    public abstract void AppendFormatted(TimeSpan t, string? format = null);
    public abstract void AppendFormatted(bool b);
    public abstract void AppendFormatted<TView>(TView v) where TView : IView;
    public abstract void AppendFormatted(Html h);
    public abstract void AppendFormatted(Slot s);
    public abstract void AppendFormatted(Action a);
    public abstract void AppendFormatted(Action<Event> a);
    public abstract void AppendFormatted(Func<Task> f);
    public abstract void AppendFormatted(Func<Event, Task> f);
}