using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Touch: UI, Subsets.Modifiers, Subsets.Touches
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Modifiers.Format + "," + 
            Subsets.Touches.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Touch> eventHandler, 
        string? format = Events.Touch.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Touch, Task> eventHandler, 
        string? format = Events.Touch.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}