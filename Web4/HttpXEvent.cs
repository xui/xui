
namespace Web4;

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
    KeyLocation Event.Keyboard.Location => Location ?? KeyLocation.Standard;
    bool Event.Keyboard.Repeat => Repeat ?? default;
    bool Event.Subsets.IsComposing.IsComposing => IsComposing ?? default;
    long Event.UI.Detail => Detail ?? default;
    bool Event.UI.Bubbles => Bubbles ?? default;
    bool Event.UI.Cancelable => Cancelable ?? default;
    bool Event.UI.Composed => Composed ?? default;
    bool Event.UI.DefaultPrevented => DefaultPrevented ?? default;
    EventPhase Event.UI.EventPhase => EventPhase ?? Web4.EventPhase.None;
    bool Event.UI.IsTrusted => IsTrusted ?? default;
    double Event.UI.TimeStamp => TimeStamp ?? default;
    Button Event.Subsets.Buttons.Button => Button ?? Web4.Button.Main;
    ButtonFlag Event.Subsets.Buttons.Buttons => Buttons ?? ButtonFlag.None;
    double Event.Subsets.X.X => X ?? default;
    double Event.Subsets.Y.Y => Y ?? default;
    double Event.Subsets.ClientXY.ClientX => ClientX ?? default;
    double Event.Subsets.ClientXY.ClientY => ClientY ?? default;
    double Event.Subsets.MovementXY.MovementX => MovementX ?? default;
    double Event.Subsets.MovementXY.MovementY => MovementY ?? default;
    double Event.Subsets.OffsetXY.OffsetX => OffsetX ?? default;
    double Event.Subsets.OffsetXY.OffsetY => OffsetY ?? default;
    double Event.Subsets.PageXY.PageX => PageX ?? default;
    double Event.Subsets.PageXY.PageY => PageY ?? default;
    double Event.Subsets.ScreenXY.ScreenX => ScreenX ?? default;
    double Event.Subsets.ScreenXY.ScreenY => ScreenY ?? default;
    bool Event.Subsets.ModifierAlt.AltKey => AltKey ?? default;
    bool Event.Subsets.ModifierCtrl.CtrlKey => CtrlKey ?? default;
    bool Event.Subsets.ModifierMeta.MetaKey => MetaKey ?? default;
    bool Event.Subsets.ModifierShift.ShiftKey => ShiftKey ?? default;
    double Event.Subsets.Deltas.DeltaX => DeltaX ?? default;
    double Event.Subsets.Deltas.DeltaY => DeltaY ?? default;
    double Event.Subsets.Deltas.DeltaZ => DeltaZ ?? default;
    DeltaMode Event.Subsets.Deltas.DeltaMode => DeltaMode ?? Web4.DeltaMode.Pixel;
    EventTarget Event.Subsets.Target.Target => Target ?? EventTarget.Empty;
    DataTransfer Event.Input.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    DataTransfer Event.Input<int>.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    DataTransfer Event.Input<long>.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    DataTransfer Event.Input<float>.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    DataTransfer Event.Input<double>.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    DataTransfer Event.Input<decimal>.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    DataTransfer Event.Input<DateTime>.DataTransfer => DataTransfer ?? DataTransfer.Empty;
    string Event.Input.InputType => InputType ?? string.Empty;
    string Event.Input<int>.InputType => InputType ?? string.Empty;
    string Event.Input<long>.InputType => InputType ?? string.Empty;
    string Event.Input<float>.InputType => InputType ?? string.Empty;
    string Event.Input<double>.InputType => InputType ?? string.Empty;
    string Event.Input<decimal>.InputType => InputType ?? string.Empty;
    string Event.Input<DateTime>.InputType => InputType ?? string.Empty;
    EventTarget<int> Event.Subsets.Target<int>.Target => new(Target);
    EventTarget<long> Event.Subsets.Target<long>.Target => new(Target);
    EventTarget<float> Event.Subsets.Target<float>.Target => new(Target);
    EventTarget<double> Event.Subsets.Target<double>.Target => new(Target);
    EventTarget<decimal> Event.Subsets.Target<decimal>.Target => new(Target);
    EventTarget<DateTime> Event.Subsets.Target<DateTime>.Target => new(Target);
    EventTarget<int> Event.Input<int>.Target => new(Target);
    EventTarget<long> Event.Input<long>.Target => new(Target);
    EventTarget<float> Event.Input<float>.Target => new(Target);
    EventTarget<double> Event.Input<double>.Target => new(Target);
    EventTarget<decimal> Event.Input<decimal>.Target => new(Target);
    EventTarget<DateTime> Event.Input<DateTime>.Target => new(Target);
    string Event.UI.Type => Type ?? string.Empty;
    EventTarget Event.UI.CurrentTarget => CurrentTarget ?? EventTarget.Empty;
    EventTarget Event.Subsets.RelatedTarget.RelatedTarget => RelatedTarget ?? EventTarget.Empty;
    TouchPoint[] Event.Subsets.Touches.ChangedTouches => ChangedTouches ?? [];
    TouchPoint[] Event.Subsets.Touches.TargetTouches => TargetTouches ?? [];
    TouchPoint[] Event.Subsets.Touches.Touches => Touches ?? [];
    string Event.Keyboard.Code => Code ?? string.Empty;
    string Event.Keyboard.Key => Key ?? string.Empty;
    string Event.Subsets.Data.Data => Data ?? string.Empty;
}
