using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Composition : UI, Subsets.Data
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Data.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Composition> listener, 
        string? format = Event.Composition.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Composition, Task> listener, 
        string? format = Event.Composition.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}