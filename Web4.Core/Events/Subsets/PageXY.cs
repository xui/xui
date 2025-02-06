using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface PageXY
                {
                    const string Format = "pageX,pageY";

                    /// <summary>
                    /// The X coordinate of the mouse pointer relative to the whole document.
                    /// </summary>
                    double PageX { get; }

                    /// <summary>
                    /// The Y coordinate of the mouse pointer relative to the whole document.
                    /// </summary>
                    double PageY { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.PageXY> listener, 
            string? format = Event.Subsets.PageXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.PageXY, Task> listener, 
            string? format = Event.Subsets.PageXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}