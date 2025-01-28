using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Web4.Composers;

public abstract class BaseComposer
{
    [ThreadStatic]
    static BaseComposer? current;
    public static BaseComposer? Current { get => current; set => current = value; }

    public int LiteralLength { get; private set; } = 0;
    public int FormattedCount { get; private set; } = 0;

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    protected bool IsInitialAppend() => FormattedCount - formattedValuesRemaining == 0;
    protected bool IsFinalAppend(string s) => literalLengthRemaining == s.Length;

    public BaseComposer Init()
    {
        LiteralLength = 0;
        FormattedCount = 0;
        return this;
    }

    public void Grow(ref Html parent, int literalLength, int formattedCount)
    {
        LiteralLength += literalLength;
        FormattedCount += formattedCount;
        
        literalLengthRemaining += literalLength;
        formattedValuesRemaining += formattedCount;

        PrepareHtml(ref parent, literalLength, formattedCount);
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
        if (literalLengthRemaining == 0 && formattedValuesRemaining == 0)
        {
            Clear();
        }
        return true;
    }

    protected virtual void Clear()
    {
        current = null;
    }

    public virtual bool WriteImmutableMarkup(ref Html parent, string literal) => CompleteStringLiteral(literal.Length);

    public virtual bool WriteMutableValue(ref Html parent, string value) => CompleteFormattedValue();
    public virtual bool WriteMutableValue(ref Html parent, bool value) => CompleteFormattedValue();
    public virtual bool WriteMutableValue<T>(ref Html parent, T value, string? format = default) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();
    
    public virtual bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();
    public virtual bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null) => CompleteFormattedValue();
    
    public virtual bool WriteEventHandler(ref Html parent, Action listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    
    public virtual bool WriteMutableElement<TView>(ref Html parent, TView view) where TView : IView => CompleteFormattedValue();
    public virtual bool WriteMutableElement(ref Html parent, Slot slot) => CompleteFormattedValue();
    public virtual bool WriteMutableElement(ref Html parent, Html partial, string? expression = null) => CompleteFormattedValue();
    public virtual void PrepareHtml(ref Html parent, int literalLength, int formattedCount) { }
}