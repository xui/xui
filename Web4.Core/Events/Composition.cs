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
        Action<Events.Composition> eventHandler, 
        string? format = Events.Composition.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
    public bool AppendFormatted(
        Func<Events.Composition, Task> eventHandler, 
        string? format = Events.Composition.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}