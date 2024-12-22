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

    public int Cursor { get; private set; } = 0;
    public int LiteralLength { get; private set; } = 0;
    public int FormattedCount { get; private set; } = 0;

    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    protected bool IsFinalAppend(string s) => literalLengthRemaining == s.Length;

    public void Grow(ref Html html, int literalLength, int formattedCount)
    {
        LiteralLength += literalLength;
        FormattedCount += formattedCount;
        
        literalLengthRemaining += literalLength;
        formattedValuesRemaining += formattedCount;

        PrepareHtml(ref html, literalLength, formattedCount);
        Cursor++;
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

    public virtual bool WriteImmutableMarkup(int index, string literal) => CompleteStringLiteral(literal.Length);

    public virtual bool WriteMutableValue(int index, string value) => CompleteFormattedValue();
    public virtual bool WriteMutableValue(int index, bool value) => CompleteFormattedValue();
    public virtual bool WriteMutableValue<T>(int index, T value, string? format = default) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();
    
    public virtual bool WriteMutableAttribute(int index, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteMutableAttribute<T>(int index, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();
    public virtual bool WriteMutableAttribute(int index, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null) => CompleteFormattedValue();
    
    public virtual bool WriteEventHandler(int index, Action eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, Action<Event> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, Func<Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, Func<Event, Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Action eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Action<Event> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Func<Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventHandler(int index, ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler, string? expression = null) => CompleteFormattedValue();
    
    public virtual bool WriteMutableElement<TView>(int index, TView view) where TView : IView => CompleteFormattedValue();
    public virtual bool WriteMutableElement(int index, Slot slot) => CompleteFormattedValue();
    public virtual bool WriteMutableElement(int index, Html partial, string? expression = null) => CompleteFormattedValue();
    public virtual void PrepareHtml(ref Html html, int literalLength, int formattedCount) { }
}