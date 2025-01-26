using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Wheel : UI, Mouse, Subsets.Deltas
    {
        new const string Format = 
            UI.Format + "," + 
            Mouse.Format + "," + 
            Subsets.Deltas.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Wheel> eventHandler, 
        string? format = Events.Wheel.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Wheel, Task> eventHandler, 
        string? format = Events.Wheel.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}