namespace Web4;

public partial interface Events
{
    public interface Pointer : UI, Mouse, Subsets.Angles, Subsets.WidthHeight, Subsets.Pressures, Subsets.Tilts
    {
        /// <summary>
        /// A unique identifier for the pointer causing the event.
        /// </summary>
        int PointerID { get; }

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
