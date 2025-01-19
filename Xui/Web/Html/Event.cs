namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

/// <summary>
/// A JavaScript event that bubbled up, beyond the browser, and over the wire to this server.
/// </summary>
/// <param name="bubbles">A boolean value indicating whether or not the event bubbles up through the DOM.</param>
/// <param name="cancelable">A boolean value indicating whether the event is cancelable.</param>
/// <param name="composed">A boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.</param>
/// <param name="currentTarget">A reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.</param>
/// <param name="defaultPrevented">Indicates whether or not the call to event.preventDefault() canceled the event.</param>
/// <param name="eventPhase">Indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.</param>
/// <param name="isTrusted">Indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).</param>
/// <param name="target">A reference to the object to which the event was originally dispatched.</param>
/// <param name="timeStamp">The time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.</param>
/// <param name="type">The name identifying the type of the event.</param>
/// <param name="detail">Returns a long with details about the event, depending on the event type.</param>
/// <param name="altKey">Returns true if the alt key was down when the mouse event was fired.</param>
/// <param name="button">The button number that was pressed (if applicable) when the mouse event was fired.</param>
/// <param name="buttons">The buttons being pressed (if any) when the mouse event was fired.</param>
/// <param name="clientX">The X coordinate of the mouse pointer in viewport coordinates.</param>
/// <param name="clientY">The Y coordinate of the mouse pointer in viewport coordinates.</param>
/// <param name="ctrlKey">Returns true if the control key was down when the mouse event was fired.</param>
/// <param name="metaKey">Returns true if the meta key was down when the mouse event was fired.</param>
/// <param name="movementX">The X coordinate of the mouse pointer relative to the position of the last mousemove event.</param>
/// <param name="movementY">The Y coordinate of the mouse pointer relative to the position of the last mousemove event.</param>
/// <param name="offsetX">The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.</param>
/// <param name="offsetY">The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.</param>
/// <param name="pageX">The X coordinate of the mouse pointer relative to the whole document.</param>
/// <param name="pageY">The Y coordinate of the mouse pointer relative to the whole document.</param>
/// <param name="relatedTarget">The secondary target for the event, if there is one.</param>
/// <param name="screenX">The X coordinate of the mouse pointer in screen coordinates.</param>
/// <param name="screenY">The Y coordinate of the mouse pointer in screen coordinates.</param>
/// <param name="shiftKey">Returns true if the shift key was down when the mouse event was fired.</param>
/// <param name="x">Alias for MouseEvent.clientX.</param>
/// <param name="y">Alias for MouseEvent.clientY.</param>
/// <param name="changedTouches">A TouchList of all the Touch objects representing individual points of contact whose states changed between the previous touch event and this one.</param>
/// <param name="targetTouches">A TouchList of all the Touch objects that are both currently in contact with the touch surface and were also started on the same element that is the target of the event.</param>
/// <param name="touches">A TouchList of all the Touch objects representing all current points of contact with the surface, regardless of target or changed status.</param>
/// <param name="code">Returns a string with the code value of the physical key represented by the event.</param>
/// <param name="isComposing">Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.</param>
/// <param name="key">Returns a string representing the key value of the key represented by the event.</param>
/// <param name="location">Returns a number representing the location of the key on the keyboard or other input device. A list of the constants identifying the locations is shown above in Keyboard locations.</param>
/// <param name="repeat">Returns a boolean value that is true if the key is being held down such that it is automatically repeating.</param>
/// <param name="deltaX">Returns a double representing the horizontal scroll amount.</param>
/// <param name="deltaY">Returns a double representing the vertical scroll amount.</param>
/// <param name="deltaZ">Returns a double representing the scroll amount for the z-axis.</param>
/// <param name="deltaMode">Returns an unsigned long representing the unit of the delta* values' scroll amount.</param>
/// <param name="data">Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).</param>
/// <param name="dataTransfer">Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.</param>
/// <param name="inputType">Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.</param>
public partial record class Event (
    // Event (base)
    bool? bubbles = null,
    bool? cancelable = null,
    bool? composed = null,
    HtmlElement? currentTarget = null,
    bool? defaultPrevented = null,
    int? eventPhase = null, // TODO: Should this be an enum? NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    bool? isTrusted = null,
    HtmlElement? target = null,
    double? timeStamp = null,
    string? type = null,

    // UIEvent
    long? detail = null,

    // MouseEvent
    bool? altKey = null,
    int? button = null,
    int? buttons = null,
    double? clientX = null,
    double? clientY = null,
    bool? ctrlKey = null,
    bool? metaKey = null,
    double? movementX = null,
    double? movementY = null,
    double? offsetX = null,
    double? offsetY = null,
    double? pageX = null,
    double? pageY = null,
    HtmlElement? relatedTarget = null,
    double? screenX = null,
    double? screenY = null,
    bool? shiftKey = null,
    double? x = null,
    double? y = null,

    // TouchEvent
    TouchPoint[]? changedTouches = null,
    TouchPoint[]? targetTouches = null,
    TouchPoint[]? touches = null,
    // Overlapping (redundant) properties:
    // bool? altKey = null,
    // bool? ctrlKey = null,
    // bool? metaKey = null,
    // bool? shiftKey = null,

    // FocusEvent
    // Overlapping (redundant) properties:
    // HtmlElement? relatedTarget = null,

    // KeyboardEvent
    string? code = null,
    bool? isComposing = null,
    string? key = null,
    long? location = null, // TODO: Enum?
    bool? repeat = null,
    // Overlapping (redundant) properties:
    // bool? altKey = null,
    // bool? ctrlKey = null,
    // bool? metaKey = null,
    // bool? shiftKey = null,

    // WheelEvent
    double? deltaX = null,
    double? deltaY = null,
    double? deltaZ = null,
    long? deltaMode = null, // TODO: Enum?

    // InputEvent
    string? data = null,
    DataTransfer? dataTransfer = null,
    string? inputType = null
    // Overlapping (redundant) properties:
    // bool? isComposing = null,

    // CompositionEvent
    // Overlapping (redundant) properties:
    // string? data = null,
//) : Event.UI, Event.Mouse, Event.Touch, Event.Focus, Event.Keyboard, Event.Wheel, Event.Input, Event.Composition {
) {
    public static readonly Event Empty = new();
}

public record class HtmlElement(
    string id = "",
    string name = "",
    string type = "",
    string value = ""
) {
    public static readonly HtmlElement Empty = new();
}

public record class DataTransfer(
    string dropEffect,
    string effectAllowed,
    string[] files,
    DataTransferItem[] items,
    string[] types
) {
    public DataTransfer() : this("", "", [], [], []) {}

    public static readonly DataTransfer Empty = new();
}

public record class DataTransferItem(
    string kind = "",
    string type = ""
) {
    public static readonly DataTransferItem Empty = new();
}

public record class TouchPoint(
    long identifier = 0,
    double screenX = 0,
    double screenY = 0,
    double clientX = 0,
    double clientY = 0,
    double pageX = 0,
    double pageY = 0
) {
    public static readonly TouchPoint Empty = new();
}

#pragma warning restore IDE1006 // Naming Styles
