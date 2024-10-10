using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.Composers;

public abstract class BaseComposer
{
    [ThreadStatic]
    static BaseComposer? current;
    public static BaseComposer? Current { get => current; set => current = value; }

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    public void Grow(int literalLength, int formattedCount)
    {
        literalLengthRemaining += literalLength;
        formattedValuesRemaining += formattedCount;
    }

    private bool CompleteStatic(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        return MoveNext();
    }

    private bool CompleteDynamic(int formattedCount)
    {
        formattedValuesRemaining -= formattedCount;
        return MoveNext();
    }

    private bool MoveNext()
    {
        if (literalLengthRemaining == 0 && formattedValuesRemaining == 0)
        {
            Clear();
        }
        return true;
    }

    private void Clear()
    {
        current = null;
    }

    public virtual bool AppendLiteral(string s) => CompleteStatic(s.Length);
    public virtual bool AppendFormatted(string s) => CompleteDynamic(1);
    public virtual bool AppendFormatted(int i, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(long l, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(float f, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(double d, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(decimal d, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(DateTime d, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(TimeSpan t, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(bool b) => CompleteDynamic(1);
    public virtual bool AppendFormatted<TView>(TView v) where TView : IView => CompleteDynamic(1);
    public virtual bool AppendFormatted(Html h) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Slot s) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Action a) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Action<Event> a) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Func<Task> f) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Func<Event, Task> f) => CompleteDynamic(1);
}