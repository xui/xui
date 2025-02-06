namespace Web4.Events;

public partial interface OneLevelRemoved
{
    public interface Input<T> : UI, Subsets.Target, Subsets.Data, Subsets.DataTransfer, Subsets.IsComposing
    {
        /// <summary>
        /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
        /// </summary>
        string InputType { get; }

        /// <summary>
        /// A reference to the object to which the event was originally dispatched.
        /// </summary>
        new EventTarget<T> Target { get; }
    }
}
