using System.Runtime.CompilerServices;
using Web4.Core.DOM;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IView : ISubset
        {
            const string Format = "view";

            /// <summary>
            /// Read-only property returns the WindowProxy object from which the event was generated. This is the Window object the event happened in.
            /// </summary>
            IWindow View { get; }
        }
    }
}