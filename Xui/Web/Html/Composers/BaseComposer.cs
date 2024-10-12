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

    // Note: formattedValuesRemaining is 1, not 0 because we kick it off with a wrapper, e.g. $"{ html() }".
    protected bool IsFinalAppend(string s) => formattedValuesRemaining == 1 && literalLengthRemaining == s.Length;
    protected bool IsFinalAppend() => formattedValuesRemaining == 1 && literalLengthRemaining == 0;

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

public static class ComposerExtensions
{
    // This strange gymnastics is required because InterpolatedStringHandlerArgument
    // must have at least one arg before it in order for the compiler to pick it up right.  
    // Extension methods help create the illusion of a simple composer.Compose($"...").
    public static void Compose(
        this BaseComposer composer, 
        [InterpolatedStringHandlerArgument("composer")] Html html)
    {
    }
}