using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IXY : ISubset, IView
        {
            new const string Format = "x,y";

            /// <summary>
            /// Alias for MouseEvent.clientX.
            /// </summary>
            double X { get; }

            /// <summary>
            /// Alias for MouseEvent.clientY.
            /// </summary>
            double Y { get; }
        }
    }
}