using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IModifierCtrl : ISubset, IView
        {
            new const string Format = "ctrlKey";

            /// <summary>
            /// Returns true if the control key was down when the event was fired.
            /// </summary>
            bool CtrlKey { get; }
        }
    }
}