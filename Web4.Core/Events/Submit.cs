namespace Web4.Events;

public partial interface OneLevelRemoved
{
    public interface Submit : Base
    {
        /// <summary>
        /// An HTMLElement object which identifies the button or other element 
        /// which was invoked to trigger the form being submitted.
        /// </summary>
        EventTarget Submitter { get; }
    }
}
