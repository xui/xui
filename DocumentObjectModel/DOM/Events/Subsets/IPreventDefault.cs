using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPreventDefault : ISubset
        {
            const string Format = "preventDefault";

            /// <summary>
            /// Cancels the event (if it is cancelable).
            /// </summary>
            void PreventDefault()
            {
            }
        }
    }
}