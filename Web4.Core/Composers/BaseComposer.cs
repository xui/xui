using System.Drawing;
using System.Runtime.CompilerServices;

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

    public bool IsBeforeAppend => FormattedCount == formattedValuesRemaining && LiteralLength == literalLengthRemaining;
    public bool IsComplete => literalLengthRemaining == 0 && formattedValuesRemaining == 1;

    public BaseComposer Init()
    {
        literalLengthRemaining = 0;
        formattedValuesRemaining = 0;
        LiteralLength = 0;
        FormattedCount = 0;
        return this;
    }

    public void Compose([InterpolatedStringHandlerArgument("")] Html html)
    {
    }

    public void Grow(int literalLength, int formattedCount)
    {
        LiteralLength += literalLength;
        FormattedCount += formattedCount;
        
        literalLengthRemaining += literalLength;
        formattedValuesRemaining += formattedCount;
    }

    public bool CompleteStringLiteral(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        return MoveNext();
    }

    public bool CompleteFormattedValue()
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

    public virtual void Clear()
    {
        current = null;
    }

    public virtual void OnHtmlPartialBegins(ref Html parent) { }
    public virtual bool OnHtmlPartialEnds(ref Html parent, scoped Html partial, string? format = null, string? expression = null)
    {
        // When the compiler instantiates the `Html partial` (above), this causes its contents to be written using the methods below due to the compiler's lowered code.
        // (more info: InterpolatedStringHandler https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        return CompleteFormattedValue();
    }

    public virtual bool WriteImmutableMarkup(ref Html parent, string literal) => CompleteStringLiteral(literal.Length);

    public virtual bool WriteMutableValue(ref Html parent, string value) => CompleteFormattedValue();
    public virtual bool WriteMutableValue(ref Html parent, bool value) => CompleteFormattedValue();
    public virtual bool WriteMutableValue(ref Html parent, Color value, string? format = null) => CompleteFormattedValue();
    public virtual bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => CompleteFormattedValue();
    public virtual bool WriteMutableValue<T>(ref Html parent, T value, string? format = null) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();

    public virtual bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();

    public virtual bool WriteMutableNode<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        foreach (var partial in partials) { }
        return CompleteFormattedValue();
    }
}