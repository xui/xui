
using System.Drawing;
using Web4.Events;
using Web4.Events.Subsets;

namespace Web4;

internal partial record class HttpXEvent(
    bool? Absolute = null,
    XYZ? Acceleration = null,
    XYZ? AccelerationIncludingGravity = null,
    double? Alpha = null,
    double? AltitudeAngle = null,
    bool? AltKey = null,
    string? AnimationName = null,
    double? AzimuthAngle = null,
    double? Beta = null,
    bool? Bubbles = null,
    Button? Button = null,
    ButtonFlag? Buttons = null,
    bool? Cancelable = null,
    TouchPoint[]? ChangedTouches = null,
    double? ClientX = null,
    double? ClientY = null,
    string? Code = null,
    int? ColNo = null,
    bool? Composed = null,
    bool? CtrlKey = null,
    EventTarget? CurrentTarget = null,
    string? Data = null,
    DataTransferContainer? DataTransfer = null,
    bool? DefaultPrevented = null,
    DeltaMode? DeltaMode = null,
    double? DeltaX = null,
    double? DeltaY = null,
    double? DeltaZ = null,
    long? Detail = null,
    float? ElapsedTime = null,
    DOMException? Error = null,
    EventPhase? EventPhase = null,
    string? FileName = null,
    double? Gamma = null,
    int? Height = null,
    string? InputType = null,
    double? Interval = null,
    bool? IsComposing = null,
    bool? IsPrimary = null,
    bool? IsTrusted = null,
    string? Key = null,
    bool? LengthComputable = null,
    int? LineNo = null,
    long? Loaded = null,
    KeyLocation? Location = null,
    string? Message = null,
    bool? MetaKey = null,
    double? MovementX = null,
    double? MovementY = null,
    string? NewUrl = null,
    double? OffsetX = null,
    double? OffsetY = null,
    string? OldUrl = null,
    double? PageX = null,
    double? PageY = null,
    int? PointerID = null,
    string? PointerType = null,
    double? Pressure = null,
    string? PropertyName = null,
    string? PseudoElement = null,
    EventTarget? RelatedTarget = null,
    bool? Repeat = null,
    ABG? RotationRate = null,
    double? ScreenX = null,
    double? ScreenY = null,
    bool? ShiftKey = null,
    EventTarget? Submitter = null,
    double? TangentialPressure = null,
    EventTarget? Target = null,
    TouchPoint[]? TargetTouches = null,
    double? TimeStamp = null,
    double? TiltX = null,
    double? TiltY = null,
    long? Total = null,
    TouchPoint[]? Touches = null,
    double? Twist = null,
    string? Type = null,
    int? Width = null,
    double? X = null,
    double? Y = null
) : Event
{
    bool IDeviceOrientation.Absolute => Absolute ?? default;
    XYZ IDeviceMotion.Acceleration => Acceleration ?? XYZ.Empty;
    XYZ IDeviceMotion.AccelerationIncludingGravity => AccelerationIncludingGravity ?? XYZ.Empty;
    double IDeviceOrientation.Alpha => Alpha ?? default;
    double IAngles.AltitudeAngle => AltitudeAngle ?? default;
    bool IModifierAlt.AltKey => AltKey ?? default;
    string IAnimation.AnimationName => AnimationName ?? string.Empty;
    double IAngles.AzimuthAngle => AzimuthAngle ?? default;
    double IDeviceOrientation.Beta => Beta ?? default;
    bool IEvent.Bubbles => Bubbles ?? default;
    Button IButtons.Button => Button ?? Web4.Events.Button.Main;
    ButtonFlag IButtons.Buttons => Buttons ?? ButtonFlag.None;
    bool IEvent.Cancelable => Cancelable ?? default;
    TouchPoint[] ITouches.ChangedTouches => ChangedTouches ?? [];
    double IClientXY.ClientX => ClientX ?? default;
    double IClientXY.ClientY => ClientY ?? default;
    string IKeys.Code => Code ?? string.Empty;
    int IError.ColNo => ColNo ?? default;
    bool IEvent.Composed => Composed ?? default;
    bool IModifierCtrl.CtrlKey => CtrlKey ?? default;
    EventTarget IEvent.CurrentTarget => CurrentTarget ?? EventTarget.Empty;
    string IData.Data => Data ?? string.Empty;
    DataTransferContainer IDataTransfer.DataTransfer => DataTransfer ?? DataTransferContainer.Empty;
    bool IEvent.DefaultPrevented => DefaultPrevented ?? default;
    DeltaMode IDeltas.DeltaMode => DeltaMode ?? Web4.Events.DeltaMode.Pixel;
    double IDeltas.DeltaX => DeltaX ?? default;
    double IDeltas.DeltaY => DeltaY ?? default;
    double IDeltas.DeltaZ => DeltaZ ?? default;
    long IDetail.Detail => Detail ?? default;
    float IAnimation.ElapsedTime => ElapsedTime ?? default;
    DOMException IError.Error => Error ?? DOMException.Empty;
    EventPhase IEvent.EventPhase => EventPhase ?? Web4.Events.EventPhase.None;
    string IError.FileName => FileName ?? string.Empty;
    double IDeviceOrientation.Gamma => Gamma ?? default;
    int IWidthHeight.Height => Height ?? default;
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
    double IDeviceMotion.Interval => Interval ?? default;
    bool IIsComposing.IsComposing => IsComposing ?? default;
    bool IPointer.IsPrimary => IsPrimary ?? default;
    bool IEvent.IsTrusted => IsTrusted ?? default;
    string IKeys.Key => Key ?? string.Empty;
    bool IProgress.LengthComputable => LengthComputable ?? default;
    int IError.LineNo => LineNo ?? default;
    long IProgress.Loaded => Loaded ?? default;
    KeyLocation IKeys.Location => Location ?? KeyLocation.Standard;
    string IError.Message => Message ?? string.Empty;
    bool IModifierMeta.MetaKey => MetaKey ?? default;
    double IMovementXY.MovementX => MovementX ?? default;
    double IMovementXY.MovementY => MovementY ?? default;
    string IHashChange.NewUrl => NewUrl ?? string.Empty;
    double IOffsetXY.OffsetX => OffsetX ?? default;
    double IOffsetXY.OffsetY => OffsetY ?? default;
    string IHashChange.OldUrl => OldUrl ?? string.Empty;
    double IPageXY.PageX => PageX ?? default;
    double IPageXY.PageY => PageY ?? default;
    int IPointer.PointerID => PointerID ?? default;
    string IPointer.PointerType => PointerType ?? string.Empty;
    double IPressures.Pressure => Pressure ?? default;
    string IAnimation.PropertyName => PropertyName ?? string.Empty;
    string IAnimation.PseudoElement => PseudoElement ?? string.Empty;
    EventTarget IRelatedTarget.RelatedTarget => RelatedTarget ?? EventTarget.Empty;
    bool IKeys.Repeat => Repeat ?? default;
    ABG IDeviceMotion.RotationRate => RotationRate ?? ABG.Empty;
    double IScreenXY.ScreenX => ScreenX ?? default;
    double IScreenXY.ScreenY => ScreenY ?? default;
    bool IModifierShift.ShiftKey => ShiftKey ?? default;
    EventTarget ISubmitter.Submitter => Submitter ?? EventTarget.Empty;
    double IPressures.TangentialPressure => TangentialPressure ?? default;
    EventTarget ITarget.Target => Target ?? EventTarget.Empty;
    EventTarget<string> ITarget<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> ITarget<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> ITarget<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> ITarget<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> ITarget<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> ITarget<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> ITarget<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> ITarget<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> ITarget<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> ITarget<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> ITarget<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> ITarget<Uri>.Target => Target ?? EventTarget.Empty;
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
    TouchPoint[] ITouches.TargetTouches => TargetTouches ?? [];
    double IEvent.TimeStamp => TimeStamp ?? default;
    double ITilts.TiltX => TiltX ?? default;
    double ITilts.TiltY => TiltY ?? default;
    long IProgress.Total => Total ?? default;
    TouchPoint[] ITouches.Touches => Touches ?? [];
    double IPointer.Twist => Twist ?? default;
    string IEvent.Type => Type ?? string.Empty;
    int IWidthHeight.Width => Width ?? default;
    double IX.X => X ?? default;
    double IY.Y => Y ?? default;
}
