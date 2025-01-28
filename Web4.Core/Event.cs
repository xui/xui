namespace Web4;

/// <summary>
/// A DOM event
/// </summary>
public partial interface Event : 
    Events,
    Events.UI,
    Events.Composition, 
    Events.Focus, 
    Events.Input, 
    Events.Input.InputInt,
    Events.Input.InputFloat,
    Events.Input.InputDouble,
    Events.Input.InputDecimal,
    Events.Input.InputDateTime,
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
    new bool? Bubbles { get; }
    
    /// <summary>
    /// A nullable boolean value indicating whether the event is cancelable.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? Cancelable { get; }
    
    /// <summary>
    /// A nullable boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? Composed { get; }

    /// <summary>
    /// A nullable reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new EventTarget? CurrentTarget { get; }

    /// <summary>
    /// A nullable boolean that indicates whether or not the call to event.preventDefault() canceled the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? DefaultPrevented { get; }

    /// <summary>
    /// A nullable int which indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new EventPhase? EventPhase { get; }
    
    /// <summary>
    /// A nullable boolean which indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? IsTrusted { get; }

    /// <summary>
    /// A nullable reference to the object to which the event was originally dispatched.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new EventTarget? Target { get; }

    /// <summary>
    /// The nullable time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? TimeStamp { get; }

    /// <summary>
    /// A nullable string indicating The name identifying the type of the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? Type { get; }

    // UIEvent
    /// <summary>
    /// Returns a nullable long with details about the event, depending on the event type.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new long? Detail { get; }

    // MouseEvent
    /// <summary>
    /// Returns true if the alt key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? AltKey { get; }

    /// <summary>
    /// The button number that was pressed (if applicable) when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new Button? Button { get; }

    /// <summary>
    /// The buttons being pressed (if any) when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new ButtonFlag? Buttons { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in viewport coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? ClientX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in viewport coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? ClientY { get; }

    /// <summary>
    /// Returns true if the control key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? CtrlKey { get; }

    /// <summary>
    /// Returns true if the meta key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? MetaKey { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? MovementX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? MovementY { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? OffsetX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? OffsetY { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the whole document.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? PageX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the whole document.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? PageY { get; }

    /// <summary>
    /// The secondary target for the event, if there is one.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new EventTarget? RelatedTarget { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in screen coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? ScreenX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in screen coordinates.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? ScreenY { get; }

    /// <summary>
    /// Returns true if the shift key was down when the mouse event was fired.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? ShiftKey { get; }

    /// <summary>
    /// Alias for MouseEvent.clientX.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? X { get; }

    /// <summary>
    /// Alias for MouseEvent.clientY.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? Y { get; }

    // TouchEvent

    /// <summary>
    /// A TouchList of all the Touch objects representing individual points of contact whose states changed between the previous touch event and this one.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new TouchPoint[]? ChangedTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects that are both currently in contact with the touch surface and were also started on the same element that is the target of the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new TouchPoint[]? TargetTouches { get; }

    /// <summary>
    /// A TouchList of all the Touch objects representing all current points of contact with the surface, regardless of target or changed status.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
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
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? Code { get; }

    /// <summary>
    /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new bool? IsComposing { get; }

    /// <summary>
    /// Returns a string representing the key value of the key represented by the event.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? Key { get; }

    /// <summary>
    /// Returns a number representing the location of the key on the keyboard or other input device.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new KeyLocation? Location { get; }

    /// <summary>
    /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
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
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? DeltaX { get; }

    /// <summary>
    /// Returns a double representing the vertical scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? DeltaY { get; }

    /// <summary>
    /// Returns a double representing the scroll amount for the z-axis.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new double? DeltaZ { get; }

    /// <summary>
    /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new DeltaMode? DeltaMode { get; }

    // InputEvent

    /// <summary>
    /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? Data { get; }

    /// <summary>
    /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new DataTransfer? DataTransfer { get; }

    /// <summary>
    /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
    /// <br/><br/>Tip: Event's interface includes fields found in all possible event types and each field is made nullable to cater to JavaScript's dynamic nature.
    /// For statically typed events, use Events.Mouse, Events.Keyboard, Events.Focus, Events.Input, Events.Touch, Events.Wheel and Events.Composition.
    /// </summary>
    new string? InputType { get; }
    // Overlapping (redundant) properties:
    // bool? isComposing { get; }

    // CompositionEvent
    // Overlapping (redundant) properties:
    // string? data { get; }

    // public static readonly Event Empty = new();
    // static abstract Event Empty();
}

public enum EventPhase : int
{
    /// <summary>
    /// The event is not being processed at this time.
    /// </summary>
    None = 0,
    /// <summary>
    /// The event is being propagated through the target's ancestor objects. 
    /// This process starts with the Window, then Document, then the HTMLHtmlElement, 
    /// and so on through the elements until the target's parent is reached. 
    /// Event listeners registered for capture mode when EventTarget.addEventListener() 
    /// was called are triggered during this phase.
    /// </summary>
    CapturingPhase = 1,
    /// <summary>
    /// The event has arrived at the event's target. Event listeners registered for 
    /// this phase are called at this time. If Event.bubbles is false, processing 
    /// the event is finished after this phase is complete.
    /// </summary>
    AtTarget = 2,
    /// <summary>
    /// The event is propagating back up through the target's ancestors in reverse order, 
    /// starting with the parent, and eventually reaching the containing Window. 
    /// This is known as bubbling, and occurs only if Event.bubbles is true. 
    /// Event listeners registered for this phase are triggered during this process.
    /// </summary>
    BubblingPhase = 3
}

public enum KeyLocation : int
{
    /// <summary>
    /// The key described by the event is not identified as being located in a particular 
    /// area of the keyboard; it is not located on the numeric keypad (unless it's the NumLock key), 
    /// and for keys that are duplicated on the left and right sides of the keyboard, the key is, 
    /// for whatever reason, not to be associated with that location.  
    /// <br/><br/>Examples include alphanumeric keys on the standard PC 101 US keyboard, 
    /// the NumLock key, and the space bar.
    /// </summary>
    Standard = 0,
    /// <summary>
    /// The key is one which may exist in multiple locations on the keyboard and, in this instance, 
    /// is on the left side of the keyboard.
    /// <br/><br/>Examples include the left Control key, the left Command key on a Macintosh 
    /// keyboard, or the left Shift key.
    /// </summary>
    Left = 1,
    /// <summary>
    /// The key is one which may exist in multiple positions on the keyboard and, in this case, 
    /// is located on the right side of the keyboard.
    /// <br/><br/>Examples include the right Shift key and the right Alt key (Option on a Mac keyboard).
    /// </summary>
    Right = 2,
    /// <summary>
    /// The key is located on the numeric keypad, or is a virtual key associated with the numeric 
    /// keypad if there's more than one place the key could originate from. The NumLock key does 
    /// not fall into this group and is always encoded with the location DOM_KEY_LOCATION_STANDARD.
    /// <br/><br/>Examples include the digits on the numeric keypad, the keypad's Enter key, and 
    /// the decimal point on the keypad.
    /// </summary>
    NumPad = 3,
}

public enum DeltaMode : int
{
    /// <summary>
    /// The delta* values are specified in pixels.
    /// </summary>
    Pixel = 0,
    /// <summary>
    /// The delta* values are specified in lines. Each mouse click scrolls a line of content, 
    /// where the method used to calculate line height is browser dependent.
    /// </summary>
    Line = 1,
    /// <summary>
    /// The delta* values are specified in pages. Each mouse click scrolls a page of content.
    /// </summary>
    Page = 2
}

public enum Button : int
{
    /// <summary>
    /// Main button pressed, usually the left button or the un-initialized state
    /// </summary>
    Main = 0,
    /// <summary>
    /// Auxiliary button pressed, usually the wheel button or the middle button (if present)
    /// </summary>
    Auxiliary = 1,
    /// <summary>
    /// Secondary button pressed, usually the right button
    /// </summary>
    Secondary = 2,
    /// <summary>
    /// Fourth button, typically the Browser Back button
    /// </summary>
    Fourth = 3,
    /// <summary>
    /// Fifth button, typically the Browser Forward button
    /// </summary>
    Fifth = 4
}

[Flags]
public enum ButtonFlag : int
{
    /// <summary>
    /// No button or un-initialized
    /// </summary>
    None = 0,
    /// <summary>
    /// Primary button (usually the left button)
    /// </summary>
    Primary = 1,
    /// <summary>
    /// Secondary button (usually the right button)
    /// </summary>
    Secondary = 2,
    /// <summary>
    /// Auxiliary button (usually the mouse wheel button or middle button)
    /// </summary>
    Auxiliary = 4,
    /// <summary>
    /// 4th button (typically the "Browser Back" button)
    /// </summary>
    Fourth = 8,
    /// <summary>
    /// 5th button (typically the "Browser Forward" button)
    /// </summary>
    Fifth = 16
}

public record class EventTarget(
    string ID = "",
    string Name = "",
    string Type = "",
    string Value = "")
{
    public static readonly EventTarget Empty = new();

    public T GetValue<T>() where T : unmanaged, IParsable<T>
        => T.TryParse(Value, null, out var value) ? value : default;

    public int? ValueAsInt => int.TryParse(Value, out int i) ? i : null;
    public float? ValueAsFloat => float.TryParse(Value, out float f) ? f : null;
    public double? ValueAsDouble => double.TryParse(Value, out double d) ? d : null;
    public decimal? ValueAsDecimal => decimal.TryParse(Value, out decimal m) ? m : null;
    public DateTime? ValueAsDateTime => DateTime.TryParse(Value, out DateTime t) ? t : null;
}

public struct IntEventTarget(EventTarget? target)
{
    public readonly string ID => target?.ID ?? "";
    public readonly string Name => target?.Name ?? "";
    public readonly string Type => target?.Type ?? "";
    public readonly int Value => target?.ValueAsInt ?? default;
}

public struct FloatEventTarget(EventTarget? target)
{
    public readonly string ID => target?.ID ?? "";
    public readonly string Name => target?.Name ?? "";
    public readonly string Type => target?.Type ?? "";
    public readonly float Value => target?.ValueAsFloat ?? default;
}

public struct DoubleEventTarget(EventTarget? target)
{
    public readonly string ID => target?.ID ?? "";
    public readonly string Name => target?.Name ?? "";
    public readonly string Type => target?.Type ?? "";
    public readonly double Value => target?.ValueAsDouble ?? default;
}

public struct DecimalEventTarget(EventTarget? target)
{
    public readonly string ID => target?.ID ?? "";
    public readonly string Name => target?.Name ?? "";
    public readonly string Type => target?.Type ?? "";
    public readonly decimal Value => target?.ValueAsDecimal ?? default;
}

public struct DateTimeEventTarget(EventTarget? target)
{
    public readonly string ID => target?.ID ?? "";
    public readonly string Name => target?.Name ?? "";
    public readonly string Type => target?.Type ?? "";
    public readonly DateTime Value => target?.ValueAsDateTime ?? default;
}

public struct EventTarget2(EventTarget? target)
{
    public readonly string ID => target?.ID ?? "";
    public readonly string Name => target?.Name ?? "";
    public readonly string Type => target?.Type ?? "";
    // public readonly T Value => T.TryParse(target?.Value ?? "", null, out var value) ? value : default;
    public T GetValue<T>() 
        where T : unmanaged, IParsable<T>
        => T.TryParse(target?.Value ?? "", null, out var value) ? value : default;
}

public record class DataTransfer(
    string DropEffect,
    string EffectAllowed,
    string[] Files,
    DataTransferItem[] Items,
    string[] Types
) {
    public DataTransfer() : this("", "", [], [], []) {}

    public static readonly DataTransfer Empty = new();
}

public record class DataTransferItem(
    string Kind = "",
    string Type = ""
) {
    public static readonly DataTransferItem Empty = new();
}

public record class TouchPoint(
    long Identifier = 0,
    double ScreenX = 0,
    double ScreenY = 0,
    double ClientX = 0,
    double ClientY = 0,
    double PageX = 0,
    double PageY = 0
) {
    public static readonly TouchPoint Empty = new();
}
