using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IDeltas : ISubset, IView
        {
            new const string Format = "deltaX,deltaY,deltaZ,deltaMode";

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

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Deltas> listener, 
            string? format = Deltas.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Deltas, Task> listener, 
            string? format = Deltas.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}