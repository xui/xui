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

    public virtual bool AppendStaticPartialMarkup(string literal) => CompleteStatic(literal.Length);

    public virtual bool AppendDynamicValue(string value) => CompleteDynamic(1);
    public virtual bool AppendDynamicValue(bool value) => CompleteDynamic(1);
    public virtual bool AppendDynamicValue<T>(T value, string? format = default) where T : struct, IUtf8SpanFormattable => CompleteDynamic(1);
    
    public virtual bool AppendDynamicAttribute(ReadOnlySpan<char> attrName, Func<Event, bool> attrValue) => CompleteDynamic(1);
    public virtual bool AppendDynamicAttribute<T>(ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null) where T : struct, IUtf8SpanFormattable => CompleteDynamic(1);
    public virtual bool AppendDynamicAttribute(ReadOnlySpan<char> attrName, Func<string, Html> attrValue) => CompleteDynamic(1);
    
    public virtual bool AppendEventHandler(Action eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(Action<Event> eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(Func<Task> eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(Func<Event, Task> eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Action<Event> eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Task> eventHandler) => CompleteDynamic(1);
    public virtual bool AppendEventHandler(ReadOnlySpan<char> attributeName, Func<Event, Task> eventHandler) => CompleteDynamic(1);
    
    public virtual bool AppendDynamicElement<TView>(TView view) where TView : IView => CompleteDynamic(1);
    public virtual bool AppendDynamicElement(Slot slot) => CompleteDynamic(1);
    public virtual bool AppendDynamicElement(Html partial) => CompleteDynamic(1);
    public virtual void PrependDynamicElement() { }
}