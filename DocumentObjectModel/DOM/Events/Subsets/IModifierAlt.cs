using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IModifierAlt : ISubset, IView
        {
            new const string Format = "altKey";

            /// <summary>
            /// Returns true if the alt key was down when the event was fired.
            /// </summary>
            bool AltKey { get; }
        }
    }
}