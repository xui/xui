namespace Web4;

public partial interface Events
{
    public interface Input<T> : UI, Subsets.Target, Subsets.Data, Subsets.IsComposing
    {
        /// <summary>
        /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
        /// </summary>
        DataTransfer DataTransfer { get; }

        /// <summary>
        /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
        /// </summary>
        string InputType { get; }

        new EventTarget<T> Target { get; }
    }
}
