namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IModifierMeta : ISubset, IView
        {
            new const string TRIM = "metaKey";

            /// <summary>
            /// Returns true if the meta key was down when the event was fired.
            /// </summary>
            bool MetaKey { get; }
        }
    }
}