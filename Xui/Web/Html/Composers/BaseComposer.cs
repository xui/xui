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

        PrepareHtml(literalLength, formattedCount);
    }

    protected bool CompleteStringLiteral(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        return MoveNext();
    }

    protected bool CompleteFormattedValue()
    {
        formattedValuesRemaining -= 1;
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

    public virtual bool AppendImmutableMarkup(string literal) => CompleteStringLiteral(literal.Length);

    public virtual bool AppendMutableValue(string value) => CompleteFormattedValue();
    public virtual bool AppendMutableValue(bool value) => CompleteFormattedValue();
    public virtual bool AppendMutableValue<T>(T value, string? format = default) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();
    
    public virtual bool AppendMutableAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendMutableAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();
    public virtual bool AppendMutableAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null) => CompleteFormattedValue();
    
    public virtual bool AppendEventHandler(Action eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(Action<Event> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(Func<Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(Func<Event, Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    
    public virtual bool AppendMutableElement<TView>(TView view) where TView : IView => CompleteFormattedValue();
    public virtual bool AppendMutableElement(Slot slot) => CompleteFormattedValue();
    public virtual bool AppendMutableElement(Html partial, string? expression = null) => CompleteFormattedValue();
    public virtual void PrepareHtml(int literalLength, int formattedCount) { }
}