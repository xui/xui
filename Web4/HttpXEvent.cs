
using System.Drawing;
using Web4.Events;
using Web4.Events.Subsets;

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
    DataTransferContainer? DataTransfer = null,
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

    EventTarget? Submitter = null,

    string? PropertyName = null,

    XYZ? Acceleration = null,
    XYZ? AccelerationIncludingGravity = null,
    ABG? RotationRate = null,
    double? Interval = null,

    bool? Absolute = null,
    double? Alpha = null,
    double? Beta = null,
    double? Gamma = null
) : Event
{
    string IAnimationSubset.AnimationName => AnimationName ?? string.Empty;
    bool IIsComposingSubset.IsComposing => IsComposing ?? default;
    long IUIEvent.Detail => Detail ?? default;
    bool IBaseEvent.Bubbles => Bubbles ?? default;
    bool IBaseEvent.Cancelable => Cancelable ?? default;
    bool IBaseEvent.Composed => Composed ?? default;
    bool IBaseEvent.DefaultPrevented => DefaultPrevented ?? default;
    EventPhase IBaseEvent.EventPhase => EventPhase ?? Web4.Events.EventPhase.None;
    bool IBaseEvent.IsTrusted => IsTrusted ?? default;
    double IBaseEvent.TimeStamp => TimeStamp ?? default;
    Button IButtonsSubset.Button => Button ?? Web4.Events.Button.Main;
    ButtonFlag IButtonsSubset.Buttons => Buttons ?? ButtonFlag.None;
    string IKeysSubset.Code => Code ?? string.Empty;
    string IKeysSubset.Key => Key ?? string.Empty;
    KeyLocation IKeysSubset.Location => Location ?? KeyLocation.Standard;
    bool IKeysSubset.Repeat => Repeat ?? default;
    double IXSubset.X => X ?? default;
    double IYSubset.Y => Y ?? default;
    double IClientXYSubset.ClientX => ClientX ?? default;
    double IClientXYSubset.ClientY => ClientY ?? default;
    double IMovementXYSubset.MovementX => MovementX ?? default;
    double IMovementXYSubset.MovementY => MovementY ?? default;
    double IOffsetXYSubset.OffsetX => OffsetX ?? default;
    double IOffsetXYSubset.OffsetY => OffsetY ?? default;
    double IPageXYSubset.PageX => PageX ?? default;
    double IPageXYSubset.PageY => PageY ?? default;
    double IScreenXYSubset.ScreenX => ScreenX ?? default;
    double IScreenXYSubset.ScreenY => ScreenY ?? default;
    bool IModifierAltSubset.AltKey => AltKey ?? default;
    bool IModifierCtrlSubset.CtrlKey => CtrlKey ?? default;
    bool IModifierMetaSubset.MetaKey => MetaKey ?? default;
    bool IModifierShiftSubset.ShiftKey => ShiftKey ?? default;
    double IDeltasSubset.DeltaX => DeltaX ?? default;
    double IDeltasSubset.DeltaY => DeltaY ?? default;
    double IDeltasSubset.DeltaZ => DeltaZ ?? default;
    DeltaMode IDeltasSubset.DeltaMode => DeltaMode ?? Web4.Events.DeltaMode.Pixel;
    EventTarget ITargetSubset.Target => Target ?? EventTarget.Empty;
    string IInputEvent<string>.InputType => InputType ?? string.Empty;
    string IInputEvent<bool>.InputType => InputType ?? string.Empty;
    string IInputEvent<Color>.InputType => InputType ?? string.Empty;
    string IInputEvent<Uri>.InputType => InputType ?? string.Empty;
    string IInputEvent<int>.InputType => InputType ?? string.Empty;
    string IInputEvent<long>.InputType => InputType ?? string.Empty;
    string IInputEvent<float>.InputType => InputType ?? string.Empty;
    string IInputEvent<double>.InputType => InputType ?? string.Empty;
    string IInputEvent<decimal>.InputType => InputType ?? string.Empty;
    string IInputEvent<DateTime>.InputType => InputType ?? string.Empty;
    string IInputEvent<DateOnly>.InputType => InputType ?? string.Empty;
    string IInputEvent<TimeOnly>.InputType => InputType ?? string.Empty;
    EventTarget<string> ITargetSubset<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> ITargetSubset<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> ITargetSubset<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> ITargetSubset<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> ITargetSubset<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> ITargetSubset<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> ITargetSubset<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> ITargetSubset<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> ITargetSubset<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> ITargetSubset<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> ITargetSubset<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> ITargetSubset<Uri>.Target => Target ?? EventTarget.Empty;
    EventTarget<string> IInputEvent<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> IInputEvent<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> IInputEvent<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> IInputEvent<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> IInputEvent<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> IInputEvent<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> IInputEvent<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> IInputEvent<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> IInputEvent<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> IInputEvent<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> IInputEvent<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> IInputEvent<Uri>.Target => Target ?? EventTarget.Empty;
    string IBaseEvent.Type => Type ?? string.Empty;
    EventTarget IBaseEvent.CurrentTarget => CurrentTarget ?? EventTarget.Empty;
    EventTarget IRelatedTargetSubset.RelatedTarget => RelatedTarget ?? EventTarget.Empty;
    TouchPoint[] ITouchesSubset.ChangedTouches => ChangedTouches ?? [];
    TouchPoint[] ITouchesSubset.TargetTouches => TargetTouches ?? [];
    TouchPoint[] ITouchesSubset.Touches => Touches ?? [];
    string IDataSubset.Data => Data ?? string.Empty;
    float IAnimationSubset.ElapsedTime => ElapsedTime ?? default;
    string IAnimationSubset.PseudoElement => PseudoElement ?? string.Empty;
    string IHashChangeSubset.NewUrl => NewUrl ?? string.Empty;
    string IHashChangeSubset.OldUrl => OldUrl ?? string.Empty;
    double IAnglesSubset.AltitudeAngle => AltitudeAngle ?? default;
    double IAnglesSubset.AzimuthAngle => AzimuthAngle ?? default;
    int IWidthHeightSubset.Width => Width ?? default;
    int IWidthHeightSubset.Height => Height ?? default;
    double IPressuresSubset.Pressure => Pressure ?? default;
    double IPressuresSubset.TangentialPressure => TangentialPressure ?? default;
    double ITiltsSubset.TiltX => TiltX ?? default;
    double ITiltsSubset.TiltY => TiltY ?? default;
    int IPointerSubset.PointerID => PointerID ?? default;
    double IPointerSubset.Twist => Twist ?? default;
    string IPointerSubset.PointerType => PointerType ?? string.Empty;
    bool IPointerSubset.IsPrimary => IsPrimary ?? default;
    bool IProgressSubset.LengthComputable => LengthComputable ?? default;
    long IProgressSubset.Loaded => Loaded ?? default;
    long IProgressSubset.Total => Total ?? default;
    EventTarget ISubmitterSubset.Submitter => Submitter ?? EventTarget.Empty;
    string IAnimationSubset.PropertyName => PropertyName ?? string.Empty;
    XYZ IDeviceMotionSubset.Acceleration => Acceleration ?? XYZ.Empty;
    XYZ IDeviceMotionSubset.AccelerationIncludingGravity => AccelerationIncludingGravity ?? XYZ.Empty;
    ABG IDeviceMotionSubset.RotationRate => RotationRate ?? ABG.Empty;
    double IDeviceMotionSubset.Interval => Interval ?? default;
    bool IDeviceOrientationSubset.Absolute => Absolute ?? default;
    double IDeviceOrientationSubset.Alpha => Alpha ?? default;
    double IDeviceOrientationSubset.Beta => Beta ?? default;
    double IDeviceOrientationSubset.Gamma => Gamma ?? default;
    DataTransferContainer IDataTransferSubset.DataTransfer => DataTransfer ?? DataTransferContainer.Empty;
}
