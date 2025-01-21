namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

/// <summary>
/// A DOM event
/// </summary>
public partial interface Event : 
    Events.UI,
    Events.Composition, 
    Events.Focus, 
    Events.Input, 
    Events.Keyboard, 
    Events.Mouse, 
    Events.Touch, 
    Events.Wheel
{
    // Event (base)
    /// <summary>
    /// A nullable boolean value indicating whether or not the event bubbles up through the DOM.  
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? bubbles { get; }
    
    /// <summary>
    /// A nullable boolean value indicating whether the event is cancelable.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? cancelable { get; }
    
    /// <summary>
    /// A nullable boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? composed { get; }

    /// <summary>
    /// A nullable reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new HtmlElement? currentTarget { get; }

    /// <summary>
    /// A nullable boolean that indicates whether or not the call to event.preventDefault() canceled the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? defaultPrevented { get; }

    /// <summary>
    /// A nullable int which indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new EventPhase? eventPhase { get; }
    
    /// <summary>
    /// A nullable boolean which indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? isTrusted { get; }

    /// <summary>
    /// A nullable reference to the object to which the event was originally dispatched.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new HtmlElement? target { get; }

    /// <summary>
    /// The nullable time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? timeStamp { get; }

    /// <summary>
    /// A nullable string indicating The name identifying the type of the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? type { get; }

    // UIEvent
    /// <summary>
    /// Returns a nullable long with details about the event, depending on the event type.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new long? detail { get; }

    // MouseEvent
    /// <summary>
    /// Returns true if the alt key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? altKey { get; }

    /// <summary>
    /// The button number that was pressed (if applicable) when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new Button? button { get; }

    /// <summary>
    /// The buttons being pressed (if any) when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new ButtonFlag? buttons { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in viewport coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? clientX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in viewport coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? clientY { get; }

    /// <summary>
    /// Returns true if the control key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? ctrlKey { get; }

    /// <summary>
    /// Returns true if the meta key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? metaKey { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? movementX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? movementY { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? offsetX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? offsetY { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the whole document.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? pageX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the whole document.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? pageY { get; }

    /// <summary>
    /// The secondary target for the event, if there is one.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new HtmlElement? relatedTarget { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in screen coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? screenX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in screen coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? screenY { get; }

    /// <summary>
    /// Returns true if the shift key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? shiftKey { get; }

    /// <summary>
    /// Alias for MouseEvent.clientX.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? x { get; }

    /// <summary>
    /// Alias for MouseEvent.clientY.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? y { get; }

    // TouchEvent

    /// <summary>
    /// A TouchList of all the Touch objects representing individual points of contact whose states changed between the previous touch event and this one.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new TouchPoint[]? changedTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects that are both currently in contact with the touch surface and were also started on the same element that is the target of the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new TouchPoint[]? targetTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects representing all current points of contact with the surface, regardless of target or changed status.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new TouchPoint[]? touches { get; }
    // Overlapping (redundant) properties:
    // bool? altKey { get; }
    // bool? ctrlKey { get; }
    // bool? metaKey { get; }
    // bool? shiftKey { get; }

    // FocusEvent
    // Overlapping (redundant) properties:
    // HtmlElement? relatedTarget { get; }

    // KeyboardEvent
    
    /// <summary>
    /// Returns a string with the code value of the physical key represented by the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? code { get; }

    /// <summary>
    /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? isComposing { get; }

    /// <summary>
    /// Returns a string representing the key value of the key represented by the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? key { get; }

    /// <summary>
    /// Returns a number representing the location of the key on the keyboard or other input device.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new KeyLocation? location { get; }

    /// <summary>
    /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? repeat { get; }
    // Overlapping (redundant) properties:
    // bool? altKey { get; }
    // bool? ctrlKey { get; }
    // bool? metaKey { get; }
    // bool? shiftKey { get; }

    // WheelEvent

    /// <summary>
    /// Returns a double representing the horizontal scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? deltaX { get; }

    /// <summary>
    /// Returns a double representing the vertical scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? deltaY { get; }

    /// <summary>
    /// Returns a double representing the scroll amount for the z-axis.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? deltaZ { get; }

    /// <summary>
    /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new DeltaMode? deltaMode { get; }

    // InputEvent

    /// <summary>
    /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? data { get; }

    /// <summary>
    /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new DataTransfer? dataTransfer { get; }

    /// <summary>
    /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? inputType { get; }
    // Overlapping (redundant) properties:
    // bool? isComposing { get; }

    // CompositionEvent
    // Overlapping (redundant) properties:
    // string? data { get; }

    // public static readonly Event Empty = new();
    // static abstract Event Empty();
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
