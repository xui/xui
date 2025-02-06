using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface Tilts
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
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.Tilts> listener, 
            string? format = Event.Subsets.Tilts.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.Tilts, Task> listener, 
            string? format = Event.Subsets.Tilts.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}