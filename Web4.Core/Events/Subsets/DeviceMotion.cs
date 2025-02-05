using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface DeviceMotion
        {
            const string Format = "acceleration,acceleratinIncludingGravity,rotationRate,interval";

            /// <summary>
            /// An object giving the acceleration of the device on the three axis X, Y and Z. 
            /// Acceleration is expressed in m/s².
            /// </summary>
            XYZ Acceleration { get; }

            /// <summary>
            /// An object giving the acceleration of the device on the three axis X, Y and Z 
            /// with the effect of gravity. Acceleration is expressed in m/s².
            /// </summary>
            XYZ AccelerationIncludingGravity { get; }

            /// <summary>
            /// An object giving the rate of change of the device's orientation on the three 
            /// orientation axis alpha, beta and gamma. Rotation rate is expressed in 
            /// degrees per seconds.
            /// </summary>
            ABG RotationRate { get; }

            /// <summary>
            /// A number representing the interval of time, in milliseconds, at which data 
            /// is obtained from the device.
            /// </summary>
            double Interval { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.DeviceMotion> listener, 
        string? format = Event.Subsets.DeviceMotion.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.DeviceMotion, Task> listener, 
        string? format = Event.Subsets.DeviceMotion.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}