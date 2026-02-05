namespace Web4.Dom.Events.Subsets;

public interface IDataTransferSubset : ISubset, IViewSubset
{
    new const string TRIM = "dataTransfer";

    /// <summary>
    /// Returns a DataTransfer object containing contextual information.
    /// </summary>
    DataTransferContainer DataTransfer { get; }
}

/// <summary>
/// The DataTransfer object is used to hold any data transferred between contexts, such as a drag and drop operation, or clipboard read/write. It may hold one or more data items, each of one or more data types.  DataTransfer was primarily designed for the HTML Drag and Drop API, as the DragEvent.dataTransfer property, and is still specified in the HTML drag-and-drop section, but it is now also used by other APIs, such as ClipboardEvent.clipboardData and InputEvent.dataTransfer. However, other APIs only use certain parts of its interface, ignoring properties such as dropEffect. Documentation of DataTransfer will primarily discuss its usage in drag-and-drop operations, and you should refer to the other APIs' documentation for usage of DataTransfer in those contexts.
/// </summary>
/// <param name="DropEffect">Gets the type of drag-and-drop operation currently selected or sets the operation to a new type. The value must be none, copy, link or move.</param>
/// <param name="EffectAllowed">Provides all of the types of operations that are possible. Must be one of none, copy, copyLink, copyMove, link, linkMove, move, all or uninitialized.</param>
/// <param name="Files">Contains a list of all the local files available on the data transfer. If the drag operation doesn't involve dragging files, this property is an empty list.</param>
/// <param name="Items">Gives a DataTransferItemList object which is a list of all of the drag data.</param>
/// <param name="Types">An array of strings giving the formats that were set in the dragstart event.</param>
public record struct DataTransferContainer(
    string DropEffect,
    string EffectAllowed,
    string[] Files,
    DataTransferItem[] Items,
    string[] Types
) {
    public DataTransferContainer() : this("", "", [], [], []) {}

    public static readonly DataTransferContainer Empty = new();
}

/// <summary>
/// The DataTransferItem object represents one drag data item. During a drag operation, each DragEvent has a dataTransfer property which contains a list of drag data items. Each item in the list is a DataTransferItem object.
/// </summary>
/// <param name="Kind">The kind of drag data item, string or file.</param>
/// <param name="Type">The drag data item's type, typically a MIME type.</param>
public record struct DataTransferItem(
    string Kind = "",
    string Type = ""
) {
    public static readonly DataTransferItem Empty = new();
}