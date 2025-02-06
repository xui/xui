using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
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
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.MovementXY> listener, 
            string? format = Event.Subsets.MovementXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.MovementXY, Task> listener, 
            string? format = Event.Subsets.MovementXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}