namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IIsComposing : ISubset, IView
        {
            new const string TRIM = "isComposing";

            /// <summary>
            /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
            /// </summary>
            bool IsComposing { get; }
        }
    }
}