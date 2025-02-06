using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface ScreenXY
                {
                    const string Format = "screenX,screenY";

                    /// <summary>
                    /// The X coordinate of the mouse pointer in screen coordinates.
                    /// </summary>
                    double ScreenX { get; }

                    /// <summary>
                    /// The Y coordinate of the mouse pointer in screen coordinates.
                    /// </summary>
                    double ScreenY { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.ScreenXY> listener, 
            string? format = Event.Subsets.ScreenXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.ScreenXY, Task> listener, 
            string? format = Event.Subsets.ScreenXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}