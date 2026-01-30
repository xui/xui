using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IRelatedTarget : ISubset, IView
        {
            new const string Format = "relatedTarget";

            /// <summary>
            /// The secondary target for the event, if there is one.
            /// </summary>
            EventTarget RelatedTarget { get; }
        }
    }
}