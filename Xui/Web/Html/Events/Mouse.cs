using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Mouse : UI, Subsets.Buttons, Subsets.Coordinates, Subsets.Modifiers, Subsets.RelatedTarget
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Buttons.Format + "," + 
            Subsets.Coordinates.Format + "," + 
            Subsets.Modifiers.Format + "," +
            Subsets.RelatedTarget.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Mouse> eventHandler, 
        string? format = Events.Mouse.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Mouse, Task> eventHandler, 
        string? format = Events.Mouse.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}