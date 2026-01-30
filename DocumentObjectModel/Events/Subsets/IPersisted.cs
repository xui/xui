using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPersisted : ISubset, IView
        {
            new const string Format = "persisted";

            /// <summary>
            /// Indicates if the document is loading from a cache.
            /// </summary>
            bool Persisted { get; }
        }
    }
}