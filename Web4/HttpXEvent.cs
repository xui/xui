
using System.Drawing;

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
    string? Type = null,

    string? AnimationName = null,
    float? ElapsedTime = null,
    string? PseudoElement = null,

    string? NewUrl = null,
    string? OldUrl = null,

    double? AltitudeAngle = null,
    double? AzimuthAngle = null,
    int? PointerID = null,
    int? Width = null,
    int? Height = null,
    double? Pressure = null,
    double? TangentialPressure = null,
    double? TiltX = null,
    double? TiltY = null,
    double? Twist = null,
    string? PointerType = null,
    bool? IsPrimary = null,

    bool? LengthComputable = null,
    long? Loaded = null,
    long? Total = null,

    EventTarget? Submitter = null
) : Event
{
    bool Event.Subsets.IsComposing.IsComposing => IsComposing ?? default;
    long Event.UI.Detail => Detail ?? default;
    bool Event.Base.Bubbles => Bubbles ?? default;
    bool Event.Base.Cancelable => Cancelable ?? default;
    bool Event.Base.Composed => Composed ?? default;
    bool Event.Base.DefaultPrevented => DefaultPrevented ?? default;
    EventPhase Event.Base.EventPhase => EventPhase ?? Web4.EventPhase.None;
    bool Event.Base.IsTrusted => IsTrusted ?? default;
    double Event.Base.TimeStamp => TimeStamp ?? default;
    Button Event.Subsets.Buttons.Button => Button ?? Web4.Button.Main;
    ButtonFlag Event.Subsets.Buttons.Buttons => Buttons ?? ButtonFlag.None;
    string Event.Subsets.Keys.Code => Code ?? string.Empty;
    string Event.Subsets.Keys.Key => Key ?? string.Empty;
    KeyLocation Event.Subsets.Keys.Location => Location ?? KeyLocation.Standard;
    bool Event.Subsets.Keys.Repeat => Repeat ?? default;
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
    DataTransfer Event.Input<string>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<bool>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<Color>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<Uri>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<int>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<long>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<float>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<double>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<decimal>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<DateTime>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<DateOnly>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    DataTransfer Event.Input<TimeOnly>.DataTransfer => DataTransfer ?? Web4.DataTransfer.Empty;
    string Event.Input<string>.InputType => InputType ?? string.Empty;
    string Event.Input<bool>.InputType => InputType ?? string.Empty;
    string Event.Input<Color>.InputType => InputType ?? string.Empty;
    string Event.Input<Uri>.InputType => InputType ?? string.Empty;
    string Event.Input<int>.InputType => InputType ?? string.Empty;
    string Event.Input<long>.InputType => InputType ?? string.Empty;
    string Event.Input<float>.InputType => InputType ?? string.Empty;
    string Event.Input<double>.InputType => InputType ?? string.Empty;
    string Event.Input<decimal>.InputType => InputType ?? string.Empty;
    string Event.Input<DateTime>.InputType => InputType ?? string.Empty;
    string Event.Input<DateOnly>.InputType => InputType ?? string.Empty;
    string Event.Input<TimeOnly>.InputType => InputType ?? string.Empty;
    EventTarget<string> Event.Subsets.Target<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> Event.Subsets.Target<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> Event.Subsets.Target<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> Event.Subsets.Target<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> Event.Subsets.Target<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> Event.Subsets.Target<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> Event.Subsets.Target<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> Event.Subsets.Target<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> Event.Subsets.Target<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> Event.Subsets.Target<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> Event.Subsets.Target<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> Event.Subsets.Target<Uri>.Target => Target ?? EventTarget.Empty;
    EventTarget<string> Event.Input<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> Event.Input<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> Event.Input<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> Event.Input<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> Event.Input<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> Event.Input<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> Event.Input<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> Event.Input<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> Event.Input<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> Event.Input<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> Event.Input<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> Event.Input<Uri>.Target => Target ?? EventTarget.Empty;
    string Event.Base.Type => Type ?? string.Empty;
    EventTarget Event.Base.CurrentTarget => CurrentTarget ?? EventTarget.Empty;
    EventTarget Event.Subsets.RelatedTarget.RelatedTarget => RelatedTarget ?? EventTarget.Empty;
    TouchPoint[] Event.Subsets.Touches.ChangedTouches => ChangedTouches ?? [];
    TouchPoint[] Event.Subsets.Touches.TargetTouches => TargetTouches ?? [];
    TouchPoint[] Event.Subsets.Touches.Touches => Touches ?? [];
    string Event.Subsets.Data.Data => Data ?? string.Empty;
    string Events.Subsets.Animation.AnimationName => AnimationName ?? string.Empty;
    float Events.Subsets.Animation.ElapsedTime => ElapsedTime ?? default;
    string Events.Subsets.Animation.PseudoElement => PseudoElement ?? string.Empty;
    string Events.Subsets.HashChange.NewUrl => NewUrl ?? string.Empty;
    string Events.Subsets.HashChange.OldUrl => OldUrl ?? string.Empty;
    double Events.Subsets.Angles.AltitudeAngle => AltitudeAngle ?? default;
    double Events.Subsets.Angles.AzimuthAngle => AzimuthAngle ?? default;
    int Events.Subsets.WidthHeight.Width => Width ?? default;
    int Events.Subsets.WidthHeight.Height => Height ?? default;
    double Events.Subsets.Pressures.Pressure => Pressure ?? default;
    double Events.Subsets.Pressures.TangentialPressure => TangentialPressure ?? default;
    double Events.Subsets.Tilts.TiltX => TiltX ?? default;
    double Events.Subsets.Tilts.TiltY => TiltY ?? default;
    int Events.Pointer.PointerID => PointerID ?? default;
    double Events.Pointer.Twist => Twist ?? default;
    string Events.Pointer.PointerType => PointerType ?? string.Empty;
    bool Events.Pointer.IsPrimary => IsPrimary ?? default;
    bool Events.Progress.LengthComputable => LengthComputable ?? default;
    long Events.Progress.Loaded => Loaded ?? default;
    long Events.Progress.Total => Total ?? default;
    EventTarget Events.Submit.Submitter => Submitter ?? EventTarget.Empty;
}
