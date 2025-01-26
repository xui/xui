using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface ClientXY
        {
            const string Format = "clientX,clientY";

            /// <summary>
            /// The X coordinate of the mouse pointer in viewport coordinates.
            /// </summary>
            double ClientX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer in viewport coordinates.
            /// </summary>
            double ClientY { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.ClientXY> eventHandler, 
        string? format = Events.Subsets.ClientXY.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.ClientXY, Task> eventHandler, 
        string? format = Events.Subsets.ClientXY.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}