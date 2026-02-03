namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface ISubmitter : ISubset, IView
        {
            new const string TRIM = "submitter";

            /// <summary>
            /// An HTMLElement object which identifies the button or other element 
            /// which was invoked to trigger the form being submitted.
            /// </summary>
            EventTarget Submitter { get; }
        }
    }
}