using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web;

public abstract class Composer
{
    [ThreadStatic]
    static Composer? current;
    public static Composer? Current { get => current; set => current = value; }

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    public void Grow(int literalLength, int formattedCount)
    {
        literalLengthRemaining += literalLength;
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

    public virtual void AppendLiteral(string s) => CompleteStatic(s.Length);
    public virtual void AppendFormatted(string s) => CompleteDynamic(1);
    public virtual void AppendFormatted(int i, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(long l, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(float f, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(double d, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(decimal d, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(DateTime d, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(TimeSpan t, string? format = null) => CompleteDynamic(1);
    public virtual void AppendFormatted(bool b) => CompleteDynamic(1);
    public virtual void AppendFormatted<TView>(TView v) where TView : IView => CompleteDynamic(1);
    public virtual void AppendFormatted(Html h) => CompleteDynamic(1);
    public virtual void AppendFormatted(Slot s) => CompleteDynamic(1);
    public virtual void AppendFormatted(Action a) => CompleteDynamic(1);
    public virtual void AppendFormatted(Action<Event> a) => CompleteDynamic(1);
    public virtual void AppendFormatted(Func<Task> f) => CompleteDynamic(1);
    public virtual void AppendFormatted(Func<Event, Task> f) => CompleteDynamic(1);
}