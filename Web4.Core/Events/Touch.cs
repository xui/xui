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
        Action<Events.Touch> listener, 
        string? format = Events.Touch.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Touch, Task> listener, 
        string? format = Events.Touch.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);
}