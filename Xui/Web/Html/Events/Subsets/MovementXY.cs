using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
         public interface MovementXY
        {
            const string Format = "movementX,movementY";

            /// <summary>
            /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
            /// </summary>
            double MovementX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
            /// </summary>
            double MovementY { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.MovementXY> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.MovementXY.Format, 
                expression);

    public bool AppendFormatted(
        Func<Events.Subsets.MovementXY, Task> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.MovementXY.Format, 
                expression);
}