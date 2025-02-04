namespace Web4;

public partial interface Events
{
    public interface Pointer : UI, Mouse
    {
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

        /// <summary>
        /// A unique identifier for the pointer causing the event.
        /// </summary>
        int PointerID { get; }

        /// <summary>
        /// The width (magnitude on the X axis), in CSS pixels, 
        /// of the contact geometry of the pointer.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height (magnitude on the Y axis), in CSS pixels, 
        /// of the contact geometry of the pointer.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The normalized pressure of the pointer input in the range 0 to 1, 
        /// where 0 and 1 represent the minimum and maximum pressure the 
        /// hardware is capable of detecting, respectively.
        /// </summary>
        double Pressure { get; }

        /// <summary>
        /// The normalized tangential pressure of the pointer input 
        /// (also known as barrel pressure or cylinder stress) in the 
        /// range -1 to 1, where 0 is the neutral position of the control.
        /// </summary>
        double TangentialPressure { get; }

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

        /// <summary>
        /// The clockwise rotation of the pointer (e.g. pen stylus) around 
        /// its major axis in degrees, with a value in the range 0 to 359.
        /// </summary>
        double Twist { get; }

        /// <summary>
        /// Indicates the device type that caused the event 
        /// (mouse, pen, touch, etc.).
        /// </summary>
        string PointerType { get; }

        /// <summary>
        /// Indicates if the pointer represents the primary pointer of 
        /// this pointer type.
        /// </summary>
        bool IsPrimary { get; }
    }
}
