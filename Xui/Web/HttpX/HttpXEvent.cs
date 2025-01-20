namespace Xui.Web.HttpX;

#pragma warning disable IDE1006 // Naming Styles

internal partial record class HttpXEvent (
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
) : Event {
    public static readonly HttpXEvent Empty = new();

    bool Events.Subsets.IsComposing.isComposing => throw new NotImplementedException();

    long Events.Keyboard.location => throw new NotImplementedException();

    bool Events.Keyboard.repeat => throw new NotImplementedException();

    double Events.Subsets.Deltas.deltaX => throw new NotImplementedException();

    double Events.Subsets.Deltas.deltaY => throw new NotImplementedException();

    double Events.Subsets.Deltas.deltaZ => throw new NotImplementedException();

    long Events.Subsets.Deltas.deltaMode => throw new NotImplementedException();

    int Events.Subsets.Buttons.button => throw new NotImplementedException();

    int Events.Subsets.Buttons.buttons => throw new NotImplementedException();

    double Events.Subsets.X.x => throw new NotImplementedException();

    double Events.Subsets.Y.y => throw new NotImplementedException();

    double Events.Subsets.ClientXY.clientX => throw new NotImplementedException();

    double Events.Subsets.ClientXY.clientY => throw new NotImplementedException();

    double Events.Subsets.MovementXY.movementX => throw new NotImplementedException();

    double Events.Subsets.MovementXY.movementY => throw new NotImplementedException();

    double Events.Subsets.OffsetXY.offsetX => throw new NotImplementedException();

    double Events.Subsets.OffsetXY.offsetY => throw new NotImplementedException();

    double Events.Subsets.PageXY.pageX => throw new NotImplementedException();

    double Events.Subsets.PageXY.pageY => throw new NotImplementedException();

    double Events.Subsets.ScreenXY.screenX => throw new NotImplementedException();

    double Events.Subsets.ScreenXY.screenY => throw new NotImplementedException();

    bool Events.Subsets.ModifierAlt.altKey => throw new NotImplementedException();

    bool Events.Subsets.ModifierCtrl.ctrlKey => throw new NotImplementedException();

    bool Events.Subsets.ModifierMeta.metaKey => throw new NotImplementedException();

    bool Events.Subsets.ModifierShift.shiftKey => throw new NotImplementedException();

    long Events.UI.detail => throw new NotImplementedException();

    bool Events.UI.bubbles => throw new NotImplementedException();

    bool Events.UI.cancelable => throw new NotImplementedException();

    bool Events.UI.composed => throw new NotImplementedException();

    bool Events.UI.defaultPrevented => throw new NotImplementedException();

    int Events.UI.eventPhase => throw new NotImplementedException();

    bool Events.UI.isTrusted => throw new NotImplementedException();

    double Events.UI.timeStamp => throw new NotImplementedException();
}

// public record class HtmlElement(
//     string id = "",
//     string name = "",
//     string type = "",
//     string value = ""
// ) {
//     public static readonly HtmlElement Empty = new();
// }

// public record class DataTransfer(
//     string dropEffect,
//     string effectAllowed,
//     string[] files,
//     DataTransferItem[] items,
//     string[] types
// ) {
//     public DataTransfer() : this("", "", [], [], []) {}

//     public static readonly DataTransfer Empty = new();
// }

// public record class DataTransferItem(
//     string kind = "",
//     string type = ""
// ) {
//     public static readonly DataTransferItem Empty = new();
// }

// public record class TouchPoint(
//     long identifier = 0,
//     double screenX = 0,
//     double screenY = 0,
//     double clientX = 0,
//     double clientY = 0,
//     double pageX = 0,
//     double pageY = 0
// ) {
//     public static readonly TouchPoint Empty = new();
// }

#pragma warning restore IDE1006 // Naming Styles
