namespace Web4
{
    namespace Events.Subsets
    {
        public interface ISubmitter : ISubset, IView
        {
            new const string Format = "submitter";

            /// <summary>
            /// An HTMLElement object which identifies the button or other element 
            /// which was invoked to trigger the form being submitted.
            /// </summary>
            EventTarget Submitter { get; }
        }
    }
}