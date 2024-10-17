using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xui.Web.Composers;

public abstract class BaseComposer
{
    [ThreadStatic]
    static BaseComposer? current;
    public static BaseComposer? Current { get => current; set => current = value; }

    public int Cursor { get; set; } = 0;

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    protected bool IsFinalAppend(string s) => literalLengthRemaining == s.Length;

    public void Grow(int literalLength, int formattedCount)
    {
        literalLengthRemaining += literalLength;
        formattedValuesRemaining += formattedCount;
    }

    protected bool CompleteStatic(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        return MoveNext();
    }

    protected bool CompleteDynamic(int formattedCount)
    {
        formattedValuesRemaining -= formattedCount;
        return MoveNext();
    }

    protected bool MoveNext()
    {
        Cursor++;
        if (literalLengthRemaining == 0 && formattedValuesRemaining == 0)
        {
            Clear();
        }
        return true;
    }

    protected virtual void Clear()
    {
        Cursor = 0;
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
    public virtual bool AppendFormatted(Func<Event, Html> attribute, string? expression = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, string? expression = null) where T : IUtf8SpanFormattable => CompleteDynamic(1);
    public virtual bool AppendFormatted(Func<Event, bool> attribute, string? expression = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Action eventHandler, string? expression = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Action<Event> eventHandler, string? expression = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Func<Task> eventHandler, string? expression = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Func<Event, Task> eventHandler, string? expression = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted<TView>(TView view) where TView : IView => CompleteDynamic(1);
    public virtual bool AppendFormatted(Slot slot) => CompleteDynamic(1);
    public virtual bool AppendFormatted(Html partial) => CompleteDynamic(1);
}