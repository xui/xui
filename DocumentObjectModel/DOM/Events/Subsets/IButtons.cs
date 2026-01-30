using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IButtons : ISubset, IView
        {
            new const string Format = "button,buttons";

            /// <summary>
            /// The button number that was pressed (if applicable) when the mouse event was fired.
            /// </summary>
            Button Button { get; }

            /// <summary>
            /// The buttons being pressed (if any) when the mouse event was fired.
            /// </summary>
            ButtonFlag Buttons { get; }
        }
    }
}