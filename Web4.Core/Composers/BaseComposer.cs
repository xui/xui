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

    public virtual void OnElementBegin(ref Html html) { }
    public virtual bool OnElementEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null) => CompleteFormattedValue();

    public virtual bool OnMarkup(ref Html parent, string literal) => CompleteStringLiteral(literal.Length);

    public virtual bool OnStringKeyhole(ref Html parent, string value) => CompleteFormattedValue();
    public virtual bool OnBoolKeyhole(ref Html parent, bool value) => CompleteFormattedValue();
    public virtual bool OnIntKeyhole(ref Html parent, int value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnLongKeyhole(ref Html parent, long value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => CompleteFormattedValue();
    public virtual bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => CompleteFormattedValue();

    public virtual bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();
    public virtual bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => CompleteFormattedValue();

    public virtual bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null) => true;
    public virtual bool OnIterate<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        // TODO: Don't be appending to self.  
        // Create a proper parent either here or in Html.cs.
        foreach (var partial in enumerable)
            partial.AppendFormatted(partial);
        return CompleteFormattedValue();
    }
    public virtual bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null) => true;
}