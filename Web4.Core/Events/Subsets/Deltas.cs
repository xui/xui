using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface Deltas
                {
                    const string Format = "deltaX,deltaY,deltaZ,deltaMode";

                    /// <summary>
                    /// Returns a double representing the horizontal scroll amount.
                    /// </summary>
                    double DeltaX { get; }

                    /// <summary>
                    /// Returns a double representing the vertical scroll amount.
                    /// </summary>
                    double DeltaY { get; }

                    /// <summary>
                    /// Returns a double representing the scroll amount for the z-axis.
                    /// </summary>
                    double DeltaZ { get; }

                    /// <summary>
                    /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
                    /// </summary>
                    DeltaMode DeltaMode { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.Deltas> listener, 
            string? format = Event.Subsets.Deltas.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.Deltas, Task> listener, 
            string? format = Event.Subsets.Deltas.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}