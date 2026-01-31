namespace Web4
{
    namespace Events.Subsets
    {
        public interface ISkipped : ISubset, IView
        {
            new const string Format = "skipped";

            /// <summary>
            /// Returns true if the user agent is skipping the element's rendering, or false otherwise.
            /// </summary>
            bool Skipped { get; }
        }
    }
}