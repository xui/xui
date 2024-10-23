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
    public int LiteralLength { get; set; } = 0;
    public int FormattedCount { get; set; } = 0;

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    protected bool IsFinalAppend(string s) => literalLengthRemaining == s.Length;

    public void Grow(int literalLength, int formattedCount)
    {
        LiteralLength += literalLength;
        FormattedCount += formattedCount;
        
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

    public virtual bool AppendLiteral(string literal) => CompleteStatic(literal.Length);
    public virtual bool AppendFormatted(string value) => CompleteDynamic(1);
    public virtual bool AppendFormatted(int value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(long value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(float value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(double value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(decimal value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(DateTime value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(TimeSpan value, string? format = null) => CompleteDynamic(1);
    public virtual bool AppendFormatted(bool value) => CompleteDynamic(1);
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
    public virtual void PrependHtml() { }
}