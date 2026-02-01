namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IDeviceMotion : ISubset, IView
        {
            new const string Format = "acceleration,acceleratinIncludingGravity,rotationRate,interval";

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

        /// <summary>
        /// The acceleration property is an object providing information about acceleration on three axis. Each axis is represented with its own property:
        /// </summary>
        /// <param name="X">Represents the acceleration upon the x axis which is the west to east axis</param>
        /// <param name="Y">Represents the acceleration upon the y axis which is the south to north axis</param>
        /// <param name="Z">Represents the acceleration upon the z axis which is the down to up axis</param>
        public record struct XYZ(
            double X = 0,
            double Y = 0,
            double Z = 0
        ) {
            public static readonly XYZ Empty = new();
        }

        /// <summary>
        /// The rotationRate property is a read only object describing the rotation rates of the device around each of its axes:
        /// </summary>
        /// <param name="Alpha">The rate at which the device is rotating about its Z axis; that is, being twisted about a line perpendicular to the screen.</param>
        /// <param name="Beta">The rate at which the device is rotating about its X axis; that is, front to back.</param>
        /// <param name="Gamma">The rate at which the device is rotating about its Y axis; that is, side to side.</param>
        public record struct ABG(
            double Alpha = 0,
            double Beta = 0,
            double Gamma = 0
        ) {
            public static readonly ABG Empty = new();
        }
    }
}