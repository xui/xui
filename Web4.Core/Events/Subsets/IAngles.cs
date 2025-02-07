using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IAngles
        {
            const string Format = "altitudeAngle,azimuthAngle";

            /// <summary>
            /// Represents the angle between a transducer (a pointer or stylus) axis 
            /// and the X-Y plane of a device screen.
            /// </summary>
            double AltitudeAngle { get; }

            /// <summary>
            /// Represents the angle between the Y-Z plane and the plane containing 
            /// both the transducer (a pointer or stylus) axis and the Y axis.
            /// </summary>
            double AzimuthAngle { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Angles> listener, 
            string? format = Angles.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Angles, Task> listener, 
            string? format = Angles.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}