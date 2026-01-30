using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IData : ISubset, IView
        {
            new const string Format = "data";

            /// <summary>
            /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
            /// </summary>
            string Data { get; }
        }
    }
}