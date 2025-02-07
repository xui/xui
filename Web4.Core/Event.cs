#pragma warning disable IDE1006 // Naming Styles

using System.Drawing;
using Web4.Events;
using Web4.Events.Subsets;

namespace Web4;

/// <summary>
/// A DOM event
/// </summary>
public partial interface Event : 
    Aliases,
    Aliases.Animation,
    Aliases.Composition, 
    Aliases.DeviceMotion,
    Aliases.DeviceOrientation,
    Aliases.Focus, 
    Aliases.HashChange,
    Aliases.Input<string>,
    Aliases.Input<bool>,
    Aliases.Input<int>,
    Aliases.Input<long>,
    Aliases.Input<float>,
    Aliases.Input<double>,
    Aliases.Input<decimal>,
    Aliases.Input<DateTime>,
    Aliases.Input<DateOnly>,
    Aliases.Input<TimeOnly>,
    Aliases.Input<Color>,
    Aliases.Input<Uri>,
    Aliases.Keyboard, 
    Aliases.Mouse, 
    Aliases.Pointer,
    Aliases.Progress,
    Aliases.Submit,
    Aliases.Touch,
    Aliases.Transition,
    Aliases.UI,
    Aliases.Wheel
{
    // Event (base)
    /// <summary>
    /// A nullable boolean value indicating whether or not the event bubbles up through the DOM.  
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? Bubbles { get; }
    
    /// <summary>
    /// A nullable boolean value indicating whether the event is cancelable.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? Cancelable { get; }
    
    /// <summary>
    /// A nullable boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? Composed { get; }

    /// <summary>
    /// A nullable reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new EventTarget? CurrentTarget { get; }

    /// <summary>
    /// A nullable boolean that indicates whether or not the call to event.preventDefault() canceled the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? DefaultPrevented { get; }

    /// <summary>
    /// A nullable int which indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new EventPhase? EventPhase { get; }
    
    /// <summary>
    /// A nullable boolean which indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? IsTrusted { get; }

    /// <summary>
    /// A nullable reference to the object to which the event was originally dispatched.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new EventTarget? Target { get; }

    /// <summary>
    /// The nullable time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? TimeStamp { get; }

    /// <summary>
    /// A nullable string indicating The name identifying the type of the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new string? Type { get; }

    // UIEvent
    /// <summary>
    /// Returns a nullable long with details about the event, depending on the event type.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new long? Detail { get; }

    // MouseEvent
    /// <summary>
    /// Returns true if the alt key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? AltKey { get; }

    /// <summary>
    /// The button number that was pressed (if applicable) when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new Button? Button { get; }

    /// <summary>
    /// The buttons being pressed (if any) when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new ButtonFlag? Buttons { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in viewport coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? ClientX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in viewport coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? ClientY { get; }

    /// <summary>
    /// Returns true if the control key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? CtrlKey { get; }

    /// <summary>
    /// Returns true if the meta key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? MetaKey { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? MovementX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? MovementY { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? OffsetX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? OffsetY { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the whole document.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? PageX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the whole document.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? PageY { get; }

    /// <summary>
    /// The secondary target for the event, if there is one.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new EventTarget? RelatedTarget { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in screen coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? ScreenX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in screen coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? ScreenY { get; }

    /// <summary>
    /// Returns true if the shift key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? ShiftKey { get; }

    /// <summary>
    /// Alias for MouseEvent.clientX.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? X { get; }

    /// <summary>
    /// Alias for MouseEvent.clientY.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? Y { get; }

    // TouchEvent

    /// <summary>
    /// A TouchList of all the Touch objects representing individual points of contact whose states changed between the previous touch event and this one.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new TouchPoint[]? ChangedTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects that are both currently in contact with the touch surface and were also started on the same element that is the target of the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new TouchPoint[]? TargetTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects representing all current points of contact with the surface, regardless of target or changed status.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new TouchPoint[]? Touches { get; }
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
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new string? Code { get; }

    /// <summary>
    /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? IsComposing { get; }

    /// <summary>
    /// Returns a string representing the key value of the key represented by the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new string? Key { get; }

    /// <summary>
    /// Returns a number representing the location of the key on the keyboard or other input device.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new KeyLocation? Location { get; }

    /// <summary>
    /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new bool? Repeat { get; }
    // Overlapping (redundant) properties:
    // bool? altKey { get; }
    // bool? ctrlKey { get; }
    // bool? metaKey { get; }
    // bool? shiftKey { get; }

    // WheelEvent

    /// <summary>
    /// Returns a double representing the horizontal scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? DeltaX { get; }

    /// <summary>
    /// Returns a double representing the vertical scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? DeltaY { get; }

    /// <summary>
    /// Returns a double representing the scroll amount for the z-axis.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new double? DeltaZ { get; }

    /// <summary>
    /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new DeltaMode? DeltaMode { get; }

    // InputEvent

    /// <summary>
    /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new string? Data { get; }

    /// <summary>
    /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new DataTransfer? DataTransfer { get; }

    /// <summary>
    /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Event.Mouse, Event.Keyboard, Event.Focus, Event.Input, Event.Touch, Event.Wheel and Event.Composition.
    /// </summary>
    new string? InputType { get; }
    // Overlapping (redundant) properties:
    // bool? isComposing { get; }

    // CompositionEvent
    // Overlapping (redundant) properties:
    // string? data { get; }

    // public static readonly Event Empty = new();
    // static abstract Event Empty();

    // AnimationEvent

    /// <summary>
    /// A string containing the value of the animation-name that generated the animation.
    /// </summary>
    new string? AnimationName { get; }

    /// <summary>
    /// A float giving the amount of time the animation has been running, in seconds, 
    /// when this event fired, excluding any time the animation was paused. 
    /// For an animationstart event, elapsedTime is 0.0 unless there was a negative 
    /// value for animation-delay, in which case the event will be fired with 
    /// elapsedTime containing (-1 * delay).
    /// </summary>
    new float? ElapsedTime { get; }

    /// <summary>
    /// A string, starting with '::', containing the name of the pseudo-element the 
    /// animation runs on. If the animation doesn't run on a pseudo-element but 
    /// on the element, an empty string: ''.
    /// </summary>
    new string? PseudoElement { get; }

    /// <summary>
    /// The new URL to which the window is navigating.
    /// </summary>
    new string? NewUrl { get; }

    /// <summary>
    /// The previous URL from which the window was navigated.
    /// </summary>
    new string? OldUrl { get; }

    /// <summary>
    /// Represents the angle between a transducer (a pointer or stylus) axis 
    /// and the X-Y plane of a device screen.
    /// </summary>
    new double? AltitudeAngle { get; }

    /// <summary>
    /// Represents the angle between the Y-Z plane and the plane containing 
    /// both the transducer (a pointer or stylus) axis and the Y axis.
    /// </summary>
    new double? AzimuthAngle { get; }

    /// <summary>
    /// A unique identifier for the pointer causing the event.
    /// </summary>
    new int? PointerID { get; }

    /// <summary>
    /// The width (magnitude on the X axis), in CSS pixels, 
    /// of the contact geometry of the pointer.
    /// </summary>
    new int? Width { get; }

    /// <summary>
    /// The height (magnitude on the Y axis), in CSS pixels, 
    /// of the contact geometry of the pointer.
    /// </summary>
    new int? Height { get; }

    /// <summary>
    /// The normalized pressure of the pointer input in the range 0 to 1, 
    /// where 0 and 1 represent the minimum and maximum pressure the 
    /// hardware is capable of detecting, respectively.
    /// </summary>
    new double? Pressure { get; }

    /// <summary>
    /// The normalized tangential pressure of the pointer input 
    /// (also known as barrel pressure or cylinder stress) in the 
    /// range -1 to 1, where 0 is the neutral position of the control.
    /// </summary>
    new double? TangentialPressure { get; }

    /// <summary>
    /// The plane angle (in degrees, in the range of -90 to 90) between 
    /// the Y–Z plane and the plane containing both the pointer 
    /// (e.g. pen stylus) axis and the Y axis.
    /// </summary>
    new double? TiltX { get; }

    /// <summary>
    /// The plane angle (in degrees, in the range of -90 to 90) between 
    /// the X–Z plane and the plane containing both the pointer 
    /// (e.g. pen stylus) axis and the X axis.
    /// </summary>
    new double? TiltY { get; }

    /// <summary>
    /// The clockwise rotation of the pointer (e.g. pen stylus) around 
    /// its major axis in degrees, with a value in the range 0 to 359.
    /// </summary>
    new double? Twist { get; }

    /// <summary>
    /// Indicates the device type that caused the event 
    /// (mouse, pen, touch, etc.).
    /// </summary>
    new string? PointerType { get; }

    /// <summary>
    /// Indicates if the pointer represents the primary pointer of 
    /// this pointer type.
    /// </summary>
    new bool? IsPrimary { get; }

    /// <summary>
    /// A boolean flag indicating if the ratio between the size of the data 
    /// already transmitted or processed (loaded), and the total size of the 
    /// data (total), is calculable. In other words, it tells if the 
    /// progress is measurable or not.
    /// </summary>
    new bool? LengthComputable { get; }

    /// <summary>
    /// A 64-bit unsigned integer indicating the size, in bytes, of the data 
    /// already transmitted or processed. The ratio can be calculated by 
    /// dividing ProgressEvent.total by the value of this property. When 
    /// downloading a resource using HTTP, this only counts the body of 
    /// the HTTP message, and doesn't include headers and other overhead. 
    /// Note that for compressed requests of unknown total size, loaded 
    /// might contain the size of the compressed, or decompressed, data, 
    /// depending on the browser. As of 2024, it contains the size of the 
    /// compressed data in Firefox, and the size of the uncompressed data 
    /// in Chrome.
    /// </summary>
    new long? Loaded { get; }

    /// <summary>
    /// A 64-bit unsigned integer indicating the total size, in bytes, 
    /// of the data being transmitted or processed. When downloading a 
    /// resource using HTTP, this value is taken from the Content-Length 
    /// response header. It only counts the body of the HTTP message, 
    /// and doesn't include headers and other overhead.
    /// </summary>
    new long? Total { get; }

    /// <summary>
    /// An HTMLElement object which identifies the button or other element 
    /// which was invoked to trigger the form being submitted.
    /// </summary>
    new EventTarget? Submitter { get; }

    /// <summary>
    /// A string containing the name CSS property associated with the transition.
    /// </summary>
    new string? PropertyName { get; }
}











public record class EventTarget(
    string ID = "",
    string Name = "",
    string Type = "",
    bool? Checked = false,
    string Value = "")
    : EventTarget<string>,
      EventTarget<bool>,
      EventTarget<int>,
      EventTarget<long>,
      EventTarget<float>,
      EventTarget<double>,
      EventTarget<decimal>,
      EventTarget<DateTime>,
      EventTarget<DateOnly>,
      EventTarget<TimeOnly>,
      EventTarget<Color>,
      EventTarget<Uri>
{
    public static readonly EventTarget Empty = new();

    public bool? ValueAsBool => Checked;
    public int? ValueAsInt => int.TryParse(Value, out var i) ? i : null;
    public long? ValueAsLong => long.TryParse(Value, out var i) ? i : null;
    public float? ValueAsFloat => float.TryParse(Value, out var f) ? f : null;
    public double? ValueAsDouble => double.TryParse(Value, out var d) ? d : null;
    public decimal? ValueAsDecimal => decimal.TryParse(Value, out var m) ? m : null;
    public DateTime? ValueAsDateTime => DateTime.TryParse(Value, out var d) ? d : null;
    public DateOnly? ValueAsDateOnly => DateOnly.TryParse(Value, out var d) ? d : null;
    public TimeOnly? ValueAsTimeOnly => TimeOnly.TryParse(Value, out var t) ? t : null;
    public Color? ValueAsColor => Value != null ? ColorTranslator.FromHtml(Value) : null; // TODO: Should "TryParse" not throw.
    public Uri? ValueAsUri => Uri.TryCreate(Value, UriKind.RelativeOrAbsolute, out var u) ? u : null;

    string EventTarget<string>.Value => Value ?? string.Empty;
    bool EventTarget<bool>.Value => ValueAsBool ?? default;
    int EventTarget<int>.Value => ValueAsInt ?? default;
    long EventTarget<long>.Value => ValueAsLong ?? default;
    float EventTarget<float>.Value => ValueAsFloat ?? default;
    double EventTarget<double>.Value => ValueAsDouble ?? default;
    decimal EventTarget<decimal>.Value => ValueAsDecimal ?? default;
    DateTime EventTarget<DateTime>.Value => ValueAsDateTime ?? default;
    DateOnly EventTarget<DateOnly>.Value => ValueAsDateOnly ?? default;
    TimeOnly EventTarget<TimeOnly>.Value => ValueAsTimeOnly ?? default;
    Color EventTarget<Color>.Value => ValueAsColor ?? default;
    Uri EventTarget<Uri>.Value => ValueAsUri ?? new Uri("about:blank");
}

public interface EventTarget<T>
{
    public string ID { get; }
    public string Name { get; }
    public string Type { get; }
    public bool? Checked { get; }
    public T Value { get; }
}

public record struct DataTransfer(
    string DropEffect,
    string EffectAllowed,
    string[] Files,
    DataTransferItem[] Items,
    string[] Types
) {
    public DataTransfer() : this("", "", [], [], []) {}

    public static readonly DataTransfer Empty = new();
}

public record struct DataTransferItem(
    string Kind = "",
    string Type = ""
) {
    public static readonly DataTransferItem Empty = new();
}

#pragma warning restore IDE1006 // Naming Styles