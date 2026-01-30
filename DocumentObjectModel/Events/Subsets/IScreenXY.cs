using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IScreenXY : ISubset, IView
        {
            new const string Format = "screenX,screenY";

            /// <summary>
            /// The X coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double ScreenX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double ScreenY { get; }
        }
    }
}