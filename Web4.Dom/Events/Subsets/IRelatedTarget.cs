namespace Web4.Dom
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