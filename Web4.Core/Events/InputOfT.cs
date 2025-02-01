using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Input<T> : UI, Subsets.Target<T>, Subsets.Data, Subsets.IsComposing
    {
        new const string Format = "dataTransfer,inputType," + 
            UI.Format + "," + 
            Subsets.Target<T>.Format + "," +
            Subsets.Data.Format + "," +
            Subsets.IsComposing.Format;
        
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

public ref partial struct Html
{
}