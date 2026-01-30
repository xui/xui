using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IModifierMeta : ISubset, IView
        {
            new const string Format = "metaKey";

            /// <summary>
            /// Returns true if the meta key was down when the event was fired.
            /// </summary>
            bool MetaKey { get; }
        }
    }
}