using System.Drawing;

namespace Web4.Composers;

public abstract class BaseComposer
{
    private int literalLengthTotal = 0;
    private int formattedCountTotal = 0;
    private int literalLengthRemaining = 0;
    private int formattedValuesRemaining = 0;

    public bool IsRootTemplate => literalLengthTotal == 0;
    
    public bool IsBeforeAppend => formattedCountTotal == formattedValuesRemaining && literalLengthTotal == literalLengthRemaining;
    
    public bool IsFinalLiteral => literalLengthRemaining == 0 && formattedValuesRemaining == 1;

    public void Grow(int literalLength, int formattedCount)
    {
        literalLengthTotal += literalLength;
        formattedCountTotal += formattedCount;
        
        literalLengthRemaining += literalLength;
        formattedValuesRemaining += formattedCount;
    }

    public bool CompleteStringLiteral(int literalLength)
    {
        literalLengthRemaining -= literalLength;
        return true;
    }

    public bool CompleteFormattedValue()
    {
        formattedValuesRemaining -= 1;
        return true;
    }

    public virtual void Reset()
    {
        // Called from the root Html's Dispose()
        literalLengthRemaining = 0;
        formattedValuesRemaining = 0;
        literalLengthTotal = 0;
        formattedCountTotal = 0;
    }

    public virtual void OnElementBegins(ref Html parent) { }
    public virtual bool OnElementEnds(ref Html parent, scoped Html partial, string? format = null, string? expression = null)
    {
        // When the compiler instantiates the `Html partial` (above), this causes its contents to be written using the methods below due to the compiler's lowered code.
        // (more info: InterpolatedStringHandler https://devblogs.microsoft.com/dotnet/string-interpolation-in-c-10-and-net-6/)
        return CompleteFormattedValue();
    }

    public virtual bool OnMarkup(ref Html parent, string literal) => CompleteStringLiteral(literal.Length);

    public virtual bool OnString(ref Html parent, string value) => CompleteFormattedValue();
    public virtual bool OnBool(ref Html parent, bool value) => CompleteFormattedValue();
    public virtual bool OnColor(ref Html parent, Color value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnUri(ref Html parent, Uri value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnValue<T>(ref Html parent, T value, string? format = null) where T : struct, IUtf8SpanFormattable => CompleteFormattedValue();

    public virtual bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();

    public virtual bool OnIterate<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        foreach (var partial in partials) { }
        return CompleteFormattedValue();
    }
}