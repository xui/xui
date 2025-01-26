using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
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

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.Deltas> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.Deltas.Format, 
                expression);

    public bool AppendFormatted(
        Func<Events.Subsets.Deltas, Task> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.Deltas.Format, 
                expression);
}