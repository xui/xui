
using System.Buffers;
using System.Drawing;
using System.Text.Json;
using Web4.Events;
using Web4.Events.Subsets;

namespace Web4;

internal partial class DefaultEvent(ReadOnlySequence<byte> message) : Event
{
    private readonly Dictionary<string, long> values = []; // 64 bit placeholder
    private readonly Dictionary<string, object> references = [];
    private bool areValuesParsed = false;
    private bool areReferencesParsed = false;

    private void EnsureParsed(bool canIgnoreReferences = false)
    {
        if (!areValuesParsed)
        {
            using (Debug.PerfCheck("Parse values only"))
            {
                Parse(canIgnoreReferences);
                areValuesParsed = true;
            }
        }
        
        if (!canIgnoreReferences)
        {
            using (Debug.PerfCheck("Parse with references"))
            {
                Parse(canIgnoreReferences);
                areReferencesParsed = true;
            }
        }
    }

    public void Parse(bool canIgnoreReferences = false)
    {
        try
        {
            var reader = new Utf8JsonReader(message);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (reader.ValueTextEquals("x"))
                    {
                        reader.Read();
                        double value = reader.GetDouble();
                        values[nameof(X)] = BitConverter.DoubleToInt64Bits(value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private object? GetReference(string propName)
    {
        EnsureParsed(canIgnoreReferences: false);
        return references.GetValueOrDefault(propName);
    }

    private bool? GetBool(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values.TryGetValue(propName, out long value) ? value != 0 : null;
    }

    private int? GetInt(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values.TryGetValue(propName, out long value) ? (int)value : null;
    }

    private long? GetLong(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values.TryGetValue(propName, out long value) ? value : null;
    }

    private float? GetFloat(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values.TryGetValue(propName, out long value) 
            ? (float)BitConverter.Int64BitsToDouble(value) 
            : null;
    }

    private double? GetDouble(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values.TryGetValue(propName, out long value) 
            ? BitConverter.Int64BitsToDouble(value) 
            : null;
    }

    public bool? Absolute => GetBool("absolute");
    bool IDeviceOrientation.Absolute => Absolute ?? default;

    public XYZ? Acceleration { get; private set; } = null; // TODO:
    XYZ IDeviceMotion.Acceleration => Acceleration ?? XYZ.Empty;

    public XYZ? AccelerationIncludingGravity { get; private set; } = null; // TODO:
    XYZ IDeviceMotion.AccelerationIncludingGravity => AccelerationIncludingGravity ?? XYZ.Empty;

    public double? Alpha => GetDouble("alpha");
    double IDeviceOrientation.Alpha => Alpha ?? default;

    public double? AltitudeAngle => GetDouble("altitudeAngle");
    double IAngles.AltitudeAngle => AltitudeAngle ?? default;

    public bool? AltKey => GetBool("altKey");
    bool IModifierAlt.AltKey => AltKey ?? default;

    public string? AnimationName => GetReference("animationName") as string;
    string IAnimation.AnimationName => AnimationName ?? string.Empty;

    public double? AzimuthAngle => GetDouble("azimuthAngle");
    double IAngles.AzimuthAngle => AzimuthAngle ?? default;

    public double? Beta => GetDouble("beta");
    double IDeviceOrientation.Beta => Beta ?? default;

    public bool? Bubbles => GetBool("bubbles");
    bool IEvent.Bubbles => Bubbles ?? default;

    public Button? Button => GetInt("button") switch { int v => (Button)v, _ => null };
    Button IButtons.Button => Button ?? Web4.Events.Button.Main;

    public ButtonFlag? Buttons => GetInt("button") switch { int v => (ButtonFlag)v, _ => null };
    ButtonFlag IButtons.Buttons => Buttons ?? ButtonFlag.None;

    public bool? Cancelable => GetBool("cancelable");
    bool IEvent.Cancelable => Cancelable ?? default;

    public TouchPoint[]? ChangedTouches { get; private set; } = null; // TODO:
    TouchPoint[] ITouches.ChangedTouches => ChangedTouches ?? [];

    public double? ClientX => GetDouble("clientX");
    double IClientXY.ClientX => ClientX ?? default;

    public double? ClientY => GetDouble("clientY");
    double IClientXY.ClientY => ClientY ?? default;

    public string? Code => GetReference("code") as string;
    string IKeys.Code => Code ?? string.Empty;

    public int? ColNo => GetInt("colNo");
    int IError.ColNo => ColNo ?? default;

    public bool? Composed => GetBool("composed");
    bool IEvent.Composed => Composed ?? default;

    public bool? CtrlKey => GetBool("ctrlKey");
    bool IModifierCtrl.CtrlKey => CtrlKey ?? default;

    public EventTarget? CurrentTarget { get; private set; } = null; // TODO:
    EventTarget IEvent.CurrentTarget => CurrentTarget ?? EventTarget.Empty;

    public string? Data => GetReference("data") as string;
    string IData.Data => Data ?? string.Empty;

    public DataTransferContainer? DataTransfer { get; private set; } = null; // TODO:
    DataTransferContainer IDataTransfer.DataTransfer => DataTransfer ?? DataTransferContainer.Empty;

    public bool? DefaultPrevented => GetBool("defaultPrevented");
    bool IEvent.DefaultPrevented => DefaultPrevented ?? default;

    public DeltaMode? DeltaMode => GetInt("deltaMode") switch { int v => (DeltaMode)v, _ => null };
    DeltaMode IDeltas.DeltaMode => DeltaMode ?? Web4.Events.DeltaMode.Pixel;

    public double? DeltaX => GetDouble("deltaX");
    double IDeltas.DeltaX => DeltaX ?? default;

    public double? DeltaY => GetDouble("deltaY");
    double IDeltas.DeltaY => DeltaY ?? default;

    public double? DeltaZ => GetDouble("deltaZ");
    double IDeltas.DeltaZ => DeltaZ ?? default;

    public long? Detail => GetLong("detail");
    long IDetail.Detail => Detail ?? default;

    public double? ElapsedTime => GetDouble("elapsedTime");
    double IAnimation.ElapsedTime => ElapsedTime ?? default;

    public DOMException? Error { get; private set; } = null; // TODO:
    DOMException IError.Error => Error ?? DOMException.Empty;

    public EventPhase? EventPhase => GetInt("eventPhase") switch { int v => (EventPhase)v, _ => null };
    EventPhase IEvent.EventPhase => EventPhase ?? Web4.Events.EventPhase.None;

    public string? FileName => GetReference("fileName") as string;
    string IError.FileName => FileName ?? string.Empty;

    public double? Gamma => GetDouble("gamma");
    double IDeviceOrientation.Gamma => Gamma ?? default;

    public int? Height => GetInt("height");
    int IWidthHeight.Height => Height ?? default;

    public string? InputType => GetReference("inputType") as string;
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

    public double? Interval => GetDouble("interval");
    double IDeviceMotion.Interval => Interval ?? default;

    public bool? IsComposing => GetBool("isComposing");
    bool IIsComposing.IsComposing => IsComposing ?? default;

    public bool? IsPrimary => GetBool("isPrimary");
    bool IPointer.IsPrimary => IsPrimary ?? default;

    public bool? IsTrusted => GetBool("isTrusted");
    bool IEvent.IsTrusted => IsTrusted ?? default;

    public string? Key => GetReference("key") as string;
    string IKeys.Key => Key ?? string.Empty;

    public int? Length => GetInt("length");
    int ILength.Length => Length ?? default;

    public bool? LengthComputable => GetBool("lengthComputable");
    bool IProgress.LengthComputable => LengthComputable ?? default;

    public int? LineNo => GetInt("lineNo");
    int IError.LineNo => LineNo ?? default;

    public long? Loaded => GetLong("loaded");
    long IProgress.Loaded => Loaded ?? default;

    public KeyLocation? Location => GetInt("location") switch { int v => (KeyLocation)v, _ => null };
    KeyLocation IKeys.Location => Location ?? KeyLocation.Standard;

    public string? Message => GetReference("message") as string;
    string IError.Message => Message ?? string.Empty;

    public bool? MetaKey => GetBool("metaKey");
    bool IModifierMeta.MetaKey => MetaKey ?? default;

    public double? MovementX => GetDouble("movementX");
    double IMovementXY.MovementX => MovementX ?? default;

    public double? MovementY => GetDouble("movementY");
    double IMovementXY.MovementY => MovementY ?? default;

    public string? NewState => GetReference("newState") as string;
    string IStates.NewState => NewState ?? string.Empty;

    public string? NewUrl => GetReference("newUrl") as string;
    string IHashChange.NewUrl => NewUrl ?? string.Empty;

    public double? OffsetX => GetDouble("offsetX");
    double IOffsetXY.OffsetX => OffsetX ?? default;

    public double? OffsetY => GetDouble("offsetY");
    double IOffsetXY.OffsetY => OffsetY ?? default;

    public string? OldState => GetReference("oldState") as string;
    string IStates.OldState => OldState ?? string.Empty;

    public string? OldUrl => GetReference("oldUrl") as string;
    string IHashChange.OldUrl => OldUrl ?? string.Empty;

    public double? PageX => GetDouble("pageX");
    double IPageXY.PageX => PageX ?? default;

    public double? PageY => GetDouble("pageY");
    double IPageXY.PageY => PageY ?? default;

    public bool? Persisted => GetBool("persisted");
    bool IPersisted.Persisted => Persisted ?? default;

    public int? PointerID => GetInt("pointerID");
    int IPointer.PointerID => PointerID ?? default;

    public string? PointerType => GetReference("pointerType") as string;
    string IPointer.PointerType => PointerType ?? string.Empty;

    public double? Pressure => GetDouble("pressure");
    double IPressures.Pressure => Pressure ?? default;

    public string? PropertyName => GetReference("propertyName") as string;
    string IAnimation.PropertyName => PropertyName ?? string.Empty;

    public string? PseudoElement => GetReference("pseudoElement") as string;
    string IAnimation.PseudoElement => PseudoElement ?? string.Empty;

    public EventTarget? RelatedTarget { get; private set; } = null; // TODO:
    EventTarget IRelatedTarget.RelatedTarget => RelatedTarget ?? EventTarget.Empty;

    public bool? Repeat => GetBool("repeat");
    bool IKeys.Repeat => Repeat ?? default;

    public ABG? RotationRate { get; private set; } = null; // TODO:
    ABG IDeviceMotion.RotationRate => RotationRate ?? ABG.Empty;

    public double? ScreenX => GetDouble("screenX");
    double IScreenXY.ScreenX => ScreenX ?? default;

    public double? ScreenY => GetDouble("screenY");
    double IScreenXY.ScreenY => ScreenY ?? default;

    public bool? ShiftKey => GetBool("shiftKey");
    bool IModifierShift.ShiftKey => ShiftKey ?? default;

    public bool? Skipped => GetBool("skipped");
    bool ISkipped.Skipped => Skipped ?? default;
    
    public EventTarget? Submitter { get; private set; } = null; // TODO:
    EventTarget ISubmitter.Submitter => Submitter ?? EventTarget.Empty;

    public double? TangentialPressure => GetDouble("tangentialPressure");
    double IPressures.TangentialPressure => TangentialPressure ?? default;

    public EventTarget? Target { get; private set; } = null; // TODO:
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

    public TouchPoint[]? TargetTouches { get; private set; } = null; // TODO:
    TouchPoint[] ITouches.TargetTouches => TargetTouches ?? [];

    public double? TimeStamp => GetDouble("timeStamp");
    double IEvent.TimeStamp => TimeStamp ?? default;

    public double? TiltX => GetDouble("tiltX");
    double ITilts.TiltX => TiltX ?? default;

    public double? TiltY => GetDouble("tiltY");
    double ITilts.TiltY => TiltY ?? default;

    public long? Total => GetLong("total");
    long IProgress.Total => Total ?? default;

    public TouchPoint[]? Touches { get; private set; } = null; // TODO:
    TouchPoint[] ITouches.Touches => Touches ?? [];

    public double? Twist => GetDouble("twist");
    double IPointer.Twist => Twist ?? default;

    public string? Type => GetReference("type") as string;
    string IEvent.Type => Type ?? string.Empty;

    public int? Width => GetInt("width");
    int IWidthHeight.Width => Width ?? default;

    public double? X => GetDouble("x");
    double IXY.X => X ?? default;

    public double? Y => GetDouble("y");
    double IXY.Y => Y ?? default;
}
