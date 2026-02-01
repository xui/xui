namespace Web4.Dom
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