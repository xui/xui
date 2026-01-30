using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IWidthHeight : ISubset, IView
        {
            new const string Format = "width,height";

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
        }
    }
}