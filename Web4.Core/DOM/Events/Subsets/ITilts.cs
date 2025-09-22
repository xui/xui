using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ITilts : ISubset
        {
            const string Format = "tiltX,tiltY";

            /// <summary>
            /// The plane angle (in degrees, in the range of -90 to 90) between 
            /// the Y–Z plane and the plane containing both the pointer 
            /// (e.g. pen stylus) axis and the Y axis.
            /// </summary>
            double TiltX { get; }

            /// <summary>
            /// The plane angle (in degrees, in the range of -90 to 90) between 
            /// the X–Z plane and the plane containing both the pointer 
            /// (e.g. pen stylus) axis and the X axis.
            /// </summary>
            double TiltY { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Tilts> listener, 
            string? format = Tilts.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Tilts, Task> listener, 
            string? format = Tilts.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}