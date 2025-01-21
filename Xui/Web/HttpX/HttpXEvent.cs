namespace Xui.Web.HttpX;

#pragma warning disable IDE1006 // Naming Styles

internal partial record class HttpXEvent(
    double? clientX = null,
    double? clientY = null,
    double? movementX = null,
    double? movementY = null,
    double? offsetX = null,
    double? offsetY = null,
    double? pageX = null,
    double? pageY = null,
    double? screenX = null,
    double? screenY = null,
    double? x = null,
    double? y = null,

    bool? altKey = null,
    bool? ctrlKey = null,
    bool? metaKey = null,
    bool? shiftKey = null,

    Button? button = null,
    ButtonFlag? buttons = null,

    HtmlElement? relatedTarget = null,

    TouchPoint[]? changedTouches = null,
    TouchPoint[]? targetTouches = null,
    TouchPoint[]? touches = null,

    double? deltaX = null,
    double? deltaY = null,
    double? deltaZ = null,
    DeltaMode? deltaMode = null,

    string? code = null,
    bool? isComposing = null,
    string? key = null,
    KeyLocation? location = null,
    bool? repeat = null,

    string? data = null,
    DataTransfer? dataTransfer = null,
    string? inputType = null,

    bool? bubbles = null,
    bool? cancelable = null,
    bool? composed = null,
    HtmlElement? currentTarget = null,
    bool? defaultPrevented = null,
    long? detail = null,
    EventPhase? eventPhase = null,
    bool? isTrusted = null,
    HtmlElement? target = null,
    double? timeStamp = null,
    string? type = null
) : Event
{
    KeyLocation Events.Keyboard.location => location ?? KeyLocation.Standard;
    bool Events.Keyboard.repeat => repeat ?? default;
    bool Events.Subsets.IsComposing.isComposing => isComposing ?? default;
    long Events.UI.detail => detail ?? default;
    bool Events.UI.bubbles => bubbles ?? default;
    bool Events.UI.cancelable => cancelable ?? default;
    bool Events.UI.composed => composed ?? default;
    bool Events.UI.defaultPrevented => defaultPrevented ?? default;
    EventPhase Events.UI.eventPhase => eventPhase ?? EventPhase.None;
    bool Events.UI.isTrusted => isTrusted ?? default;
    double Events.UI.timeStamp => timeStamp ?? default;
    Button Events.Subsets.Buttons.button => button ?? Button.Main;
    ButtonFlag Events.Subsets.Buttons.buttons => buttons ?? ButtonFlag.None;
    double Events.Subsets.X.x => x ?? default;
    double Events.Subsets.Y.y => y ?? default;
    double Events.Subsets.ClientXY.clientX => clientX ?? default;
    double Events.Subsets.ClientXY.clientY => clientY ?? default;
    double Events.Subsets.MovementXY.movementX => movementX ?? default;
    double Events.Subsets.MovementXY.movementY => movementY ?? default;
    double Events.Subsets.OffsetXY.offsetX => offsetX ?? default;
    double Events.Subsets.OffsetXY.offsetY => offsetY ?? default;
    double Events.Subsets.PageXY.pageX => pageX ?? default;
    double Events.Subsets.PageXY.pageY => pageY ?? default;
    double Events.Subsets.ScreenXY.screenX => screenX ?? default;
    double Events.Subsets.ScreenXY.screenY => screenY ?? default;
    bool Events.Subsets.ModifierAlt.altKey => altKey ?? default;
    bool Events.Subsets.ModifierCtrl.ctrlKey => ctrlKey ?? default;
    bool Events.Subsets.ModifierMeta.metaKey => metaKey ?? default;
    bool Events.Subsets.ModifierShift.shiftKey => shiftKey ?? default;
    double Events.Subsets.Deltas.deltaX => deltaX ?? default;
    double Events.Subsets.Deltas.deltaY => deltaY ?? default;
    double Events.Subsets.Deltas.deltaZ => deltaZ ?? default;
    DeltaMode Events.Subsets.Deltas.deltaMode => deltaMode ?? DeltaMode.Pixel;
    HtmlElement Events.UI.target => target ?? HtmlElement.Empty;
    string Events.UI.type => type ?? string.Empty;
    HtmlElement Events.UI.currentTarget => currentTarget ?? HtmlElement.Empty;
    HtmlElement Events.Subsets.RelatedTarget.relatedTarget => relatedTarget ?? HtmlElement.Empty;
    TouchPoint[] Events.Subsets.Touches.changedTouches => changedTouches ?? [];
    TouchPoint[] Events.Subsets.Touches.targetTouches => targetTouches ?? [];
    TouchPoint[] Events.Subsets.Touches.touches => touches ?? [];
    string Events.Keyboard.code => code ?? string.Empty;
    string Events.Keyboard.key => key ?? string.Empty;
    string Events.Subsets.Data.data => data ?? string.Empty;
    DataTransfer Events.Input.dataTransfer => dataTransfer ?? DataTransfer.Empty;
    string Events.Input.inputType => inputType ?? string.Empty;
}

#pragma warning restore IDE1006 // Naming Styles
