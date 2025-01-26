namespace Web4.HttpX;

internal partial record class HttpXEvent(
    double? ClientX = null,
    double? ClientY = null,
    double? MovementX = null,
    double? MovementY = null,
    double? OffsetX = null,
    double? OffsetY = null,
    double? PageX = null,
    double? PageY = null,
    double? ScreenX = null,
    double? ScreenY = null,
    double? X = null,
    double? Y = null,

    bool? AltKey = null,
    bool? CtrlKey = null,
    bool? MetaKey = null,
    bool? ShiftKey = null,

    Button? Button = null,
    ButtonFlag? Buttons = null,

    EventTarget? RelatedTarget = null,

    TouchPoint[]? ChangedTouches = null,
    TouchPoint[]? TargetTouches = null,
    TouchPoint[]? Touches = null,

    double? DeltaX = null,
    double? DeltaY = null,
    double? DeltaZ = null,
    DeltaMode? DeltaMode = null,

    string? Code = null,
    bool? IsComposing = null,
    string? Key = null,
    KeyLocation? Location = null,
    bool? Repeat = null,

    string? Data = null,
    DataTransfer? DataTransfer = null,
    string? InputType = null,

    bool? Bubbles = null,
    bool? Cancelable = null,
    bool? Composed = null,
    EventTarget? CurrentTarget = null,
    bool? DefaultPrevented = null,
    long? Detail = null,
    EventPhase? EventPhase = null,
    bool? IsTrusted = null,
    EventTarget? Target = null,
    double? TimeStamp = null,
    string? Type = null
) : Event
{
    KeyLocation Events.Keyboard.Location => Location ?? KeyLocation.Standard;
    bool Events.Keyboard.Repeat => Repeat ?? default;
    bool Events.Subsets.IsComposing.IsComposing => IsComposing ?? default;
    long Events.UI.Detail => Detail ?? default;
    bool Events.UI.Bubbles => Bubbles ?? default;
    bool Events.UI.Cancelable => Cancelable ?? default;
    bool Events.UI.Composed => Composed ?? default;
    bool Events.UI.DefaultPrevented => DefaultPrevented ?? default;
    EventPhase Events.UI.EventPhase => EventPhase ?? Web4.EventPhase.None;
    bool Events.UI.IsTrusted => IsTrusted ?? default;
    double Events.UI.TimeStamp => TimeStamp ?? default;
    Button Events.Subsets.Buttons.Button => Button ?? Web4.Button.Main;
    ButtonFlag Events.Subsets.Buttons.Buttons => Buttons ?? ButtonFlag.None;
    double Events.Subsets.X.X => X ?? default;
    double Events.Subsets.Y.Y => Y ?? default;
    double Events.Subsets.ClientXY.ClientX => ClientX ?? default;
    double Events.Subsets.ClientXY.ClientY => ClientY ?? default;
    double Events.Subsets.MovementXY.MovementX => MovementX ?? default;
    double Events.Subsets.MovementXY.MovementY => MovementY ?? default;
    double Events.Subsets.OffsetXY.OffsetX => OffsetX ?? default;
    double Events.Subsets.OffsetXY.OffsetY => OffsetY ?? default;
    double Events.Subsets.PageXY.PageX => PageX ?? default;
    double Events.Subsets.PageXY.PageY => PageY ?? default;
    double Events.Subsets.ScreenXY.ScreenX => ScreenX ?? default;
    double Events.Subsets.ScreenXY.ScreenY => ScreenY ?? default;
    bool Events.Subsets.ModifierAlt.AltKey => AltKey ?? default;
    bool Events.Subsets.ModifierCtrl.CtrlKey => CtrlKey ?? default;
    bool Events.Subsets.ModifierMeta.MetaKey => MetaKey ?? default;
    bool Events.Subsets.ModifierShift.ShiftKey => ShiftKey ?? default;
    double Events.Subsets.Deltas.DeltaX => DeltaX ?? default;
    double Events.Subsets.Deltas.DeltaY => DeltaY ?? default;
    double Events.Subsets.Deltas.DeltaZ => DeltaZ ?? default;
    DeltaMode Events.Subsets.Deltas.DeltaMode => DeltaMode ?? Web4.DeltaMode.Pixel;
    EventTarget Events.UI.Target => Target ?? EventTarget.Empty;
    string Events.UI.Type => Type ?? string.Empty;
    EventTarget Events.UI.CurrentTarget => CurrentTarget ?? EventTarget.Empty;
    EventTarget Events.Subsets.RelatedTarget.RelatedTarget => RelatedTarget ?? EventTarget.Empty;
    TouchPoint[] Events.Subsets.Touches.ChangedTouches => ChangedTouches ?? [];
    TouchPoint[] Events.Subsets.Touches.TargetTouches => TargetTouches ?? [];
    TouchPoint[] Events.Subsets.Touches.Touches => Touches ?? [];
    string Events.Keyboard.Code => Code ?? string.Empty;
    string Events.Keyboard.Key => Key ?? string.Empty;
    string Events.Subsets.Data.Data => Data ?? string.Empty;
    DataTransfer Events.Input.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    string Events.Input.InputType => InputType ?? string.Empty;
}
