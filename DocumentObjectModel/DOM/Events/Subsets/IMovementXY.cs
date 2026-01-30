using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IMovementXY : ISubset, IView
        {
            new const string Format = "movementX,movementY";

            /// <summary>
            /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
            /// </summary>
            double MovementX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
            /// </summary>
            double MovementY { get; }
        }
    }
}