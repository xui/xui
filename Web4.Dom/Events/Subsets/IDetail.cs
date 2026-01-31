namespace Web4
{
    namespace Events.Subsets
    {
        public interface IDetail : ISubset, IView
        {
            new const string Format = "detail";

            /// <summary>
            /// Returns a long with details about the event, depending on the event type.
            /// </summary>
            long Detail { get; }
        }
    }
}