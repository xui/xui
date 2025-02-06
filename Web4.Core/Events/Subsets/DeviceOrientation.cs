using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface DeviceOrientation
                {
                    const string Format = "absolute,alpha,beta,gamma";

                    /// <summary>
                    /// A boolean that indicates whether or not the device is providing 
                    /// orientation data absolutely.
                    /// </summary>
                    bool Absolute { get; }

                    /// <summary>
                    /// A number representing the motion of the device around the z axis, 
                    /// express in degrees with values ranging from 0 (inclusive) to 
                    /// 360 (exclusive).
                    /// </summary>
                    double Alpha { get; }

                    /// <summary>
                    /// A number representing the motion of the device around the x axis, 
                    /// express in degrees with values ranging from -180 (inclusive) to 
                    /// 180 (exclusive). This represents a front to back motion of the 
                    /// device.
                    /// </summary>
                    double Beta { get; }

                    /// <summary>
                    /// A number representing the motion of the device around the y axis, 
                    /// express in degrees with values ranging from -90 (inclusive) to 
                    /// 90 (exclusive). This represents a left to right motion of the 
                    /// device.
                    /// </summary>
                    double Gamma { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.DeviceOrientation> listener, 
            string? format = Event.Subsets.DeviceOrientation.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.DeviceOrientation, Task> listener, 
            string? format = Event.Subsets.DeviceOrientation.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}