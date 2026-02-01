namespace Web4.Dom
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