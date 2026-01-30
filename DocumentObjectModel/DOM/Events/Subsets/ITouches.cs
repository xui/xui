using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ITouches : ISubset, IView
        {
            new const string Format = "changedTouches,targetTouches,touches";

            /// <summary>
            /// A TouchList of all the Touch objects representing individual points of contact 
            /// whose states changed between the previous touch event and this one.
            /// </summary>
            TouchPoint[] ChangedTouches { get; }

            /// <summary>
            /// A TouchList of all the Touch objects that are both currently in contact with 
            /// the touch surface and were also started on the same element that is the target 
            /// of the event.
            /// </summary>
            TouchPoint[] TargetTouches { get; }

            /// <summary>
            /// A TouchList of all the Touch objects representing all current points of 
            /// contact with the surface, regardless of target or changed status.
            /// </summary>
            TouchPoint[] Touches { get; }
        }

        /// <summary>
        /// The Touch interface represents a single contact point on a touch-sensitive device. The contact point is commonly a finger or stylus and the device may be a touchscreen or trackpad.
        /// </summary>
        /// <param name="Identifier">Returns a unique identifier for this Touch object. A given touch point (say, by a finger) will have the same identifier for the duration of its movement around the surface. This lets you ensure that you're tracking the same touch all the time.</param>
        /// <param name="ScreenX">Returns the X coordinate of the touch point relative to the left edge of the screen.</param>
        /// <param name="ScreenY">Returns the Y coordinate of the touch point relative to the top edge of the screen.</param>
        /// <param name="ClientX">Returns the X coordinate of the touch point relative to the left edge of the browser viewport, not including any scroll offset.</param>
        /// <param name="ClientY">Returns the Y coordinate of the touch point relative to the top edge of the browser viewport, not including any scroll offset.</param>
        /// <param name="PageX">Returns the X coordinate of the touch point relative to the left edge of the document. Unlike clientX, this value includes the horizontal scroll offset, if any.</param>
        /// <param name="PageY">Returns the Y coordinate of the touch point relative to the top of the document. Unlike clientY, this value includes the vertical scroll offset, if any.</param>
        /// <param name="RadiusX">Returns the X radius of the ellipse that most closely circumscribes the area of contact with the screen. The value is in pixels of the same scale as screenX.</param>
        /// <param name="RadiusY">Returns the Y radius of the ellipse that most closely circumscribes the area of contact with the screen. The value is in pixels of the same scale as screenY.</param>
        /// <param name="RotationAngle">Returns the angle (in degrees) that the ellipse described by radiusX and radiusY must be rotated, clockwise, to most accurately cover the area of contact between the user and the surface.</param>
        /// <param name="Force">Returns the amount of pressure being applied to the surface by the user, as a float between 0.0 (no pressure) and 1.0 (maximum pressure).</param>
        public record struct TouchPoint(
            long Identifier = 0,
            double ScreenX = 0,
            double ScreenY = 0,
            double ClientX = 0,
            double ClientY = 0,
            double PageX = 0,
            double PageY = 0,
            double RadiusX = 0,
            double RadiusY = 0,
            double RotationAngle = 0,
            double Force = 0
        ) {
            public static readonly TouchPoint Empty = new();
        }
    }
}