using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
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
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.ClientXY> listener, 
            string? format = Event.Subsets.ClientXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.ClientXY, Task> listener, 
            string? format = Event.Subsets.ClientXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}