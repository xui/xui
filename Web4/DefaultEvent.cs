
using System.Buffers;
using System.Drawing;
using System.Text.Json;
using Web4.Events;
using Web4.Events.Subsets;

namespace Web4;

internal partial class DefaultEvent(ReadOnlySequence<byte> message) : Event
{
    public void Parse(bool canIgnoreReferences = false)
    {
        double d = 1.23;
        var raw1 = BitConverter.DoubleToUInt64Bits(d);
        var raw2 = BitConverter.DoubleToInt64Bits(d);

        try
        {
            var reader = new Utf8JsonReader(message);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("method"))
                {
                    reader.Read();
                    ReadOnlySpan<byte> value = reader.HasValueSequence
                        ? reader.ValueSequence.ToArray()
                        : reader.ValueSpan;
                    // key = Keymaker.GetKeyIfCached(value);
                }
Console.Write(reader.TokenType);

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                    case JsonTokenType.String:
                        {
                            string? text = reader.GetString();
Console.Write(" ");
Console.Write(text);
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            int intValue = reader.GetInt32();
Console.Write(" ");
Console.Write(intValue);
                            break;
                        }
                }
Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }




    private bool areValuesParsed = false;
    private bool areReferencesParsed = false;
    private readonly Dictionary<string, long>? values = []; // 64 bit placeholder
    private readonly Dictionary<string, object>? references = [];

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

    private object? GetReference(string propName)
    {
        EnsureParsed(canIgnoreReferences: false);
        return references!.GetValueOrDefault(propName);
    }

    private bool? GetBool(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values!.TryGetValue(nameof(propName), out long value) ? value != 0 : null;
    }

    private int? GetInt(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values!.TryGetValue(nameof(propName), out long value) ? (int)value : null;
    }

    private long? GetLong(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values!.TryGetValue(nameof(propName), out long value) ? value : null;
    }

    private float? GetFloat(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values!.TryGetValue(nameof(propName), out long value) 
            ? (float)BitConverter.Int64BitsToDouble(value) 
            : null;
    }

    private double? GetDouble(string propName)
    {
        EnsureParsed(canIgnoreReferences: true);
        return values!.TryGetValue(nameof(propName), out long value) 
            ? BitConverter.Int64BitsToDouble(value) 
            : null;
    }

    public bool? Absolute => GetBool(nameof(Absolute));
    bool IDeviceOrientation.Absolute => Absolute ?? default;

    public XYZ? Acceleration { get; private set; } = null; // TODO:
    XYZ IDeviceMotion.Acceleration => Acceleration ?? XYZ.Empty;

    public XYZ? AccelerationIncludingGravity { get; private set; } = null; // TODO:
    XYZ IDeviceMotion.AccelerationIncludingGravity => AccelerationIncludingGravity ?? XYZ.Empty;

    public double? Alpha => GetDouble(nameof(Alpha));
    double IDeviceOrientation.Alpha => Alpha ?? default;

    public double? AltitudeAngle => GetDouble(nameof(AltitudeAngle));
    double IAngles.AltitudeAngle => AltitudeAngle ?? default;

    public bool? AltKey => GetBool(nameof(AltKey));
    bool IModifierAlt.AltKey => AltKey ?? default;

    public string? AnimationName => GetReference(nameof(AnimationName)) as string;
    string IAnimation.AnimationName => AnimationName ?? string.Empty;

    public double? AzimuthAngle => GetDouble(nameof(AzimuthAngle));
    double IAngles.AzimuthAngle => AzimuthAngle ?? default;

    public double? Beta => GetDouble(nameof(Beta));
    double IDeviceOrientation.Beta => Beta ?? default;

    public bool? Bubbles => GetBool(nameof(Bubbles));
    bool IEvent.Bubbles => Bubbles ?? default;

    public Button? Button => GetInt(nameof(Button)) switch { int v => (Button)v, _ => null };
    Button IButtons.Button => Button ?? Web4.Events.Button.Main;

    public ButtonFlag? Buttons => GetInt(nameof(Button)) switch { int v => (ButtonFlag)v, _ => null };
    ButtonFlag IButtons.Buttons => Buttons ?? ButtonFlag.None;

    public bool? Cancelable => GetBool(nameof(Cancelable));
    bool IEvent.Cancelable => Cancelable ?? default;

    public TouchPoint[]? ChangedTouches { get; private set; } = null; // TODO:
    TouchPoint[] ITouches.ChangedTouches => ChangedTouches ?? [];

    public double? ClientX => GetDouble(nameof(ClientX));
    double IClientXY.ClientX => ClientX ?? default;

    public double? ClientY => GetDouble(nameof(ClientY));
    double IClientXY.ClientY => ClientY ?? default;

    public string? Code => GetReference(nameof(Code)) as string;
    string IKeys.Code => Code ?? string.Empty;

    public int? ColNo => GetInt(nameof(ColNo));
    int IError.ColNo => ColNo ?? default;

    public bool? Composed => GetBool(nameof(Composed));
    bool IEvent.Composed => Composed ?? default;

    public bool? CtrlKey => GetBool(nameof(CtrlKey));
    bool IModifierCtrl.CtrlKey => CtrlKey ?? default;

    public EventTarget? CurrentTarget { get; private set; } = null; // TODO:
    EventTarget IEvent.CurrentTarget => CurrentTarget ?? EventTarget.Empty;

    public string? Data => GetReference(nameof(Data)) as string;
    string IData.Data => Data ?? string.Empty;

    public DataTransferContainer? DataTransfer { get; private set; } = null; // TODO:
    DataTransferContainer IDataTransfer.DataTransfer => DataTransfer ?? DataTransferContainer.Empty;

    public bool? DefaultPrevented => GetBool(nameof(DefaultPrevented));
    bool IEvent.DefaultPrevented => DefaultPrevented ?? default;

    public DeltaMode? DeltaMode => GetInt(nameof(DeltaMode)) switch { int v => (DeltaMode)v, _ => null };
    DeltaMode IDeltas.DeltaMode => DeltaMode ?? Web4.Events.DeltaMode.Pixel;

    public double? DeltaX => GetDouble(nameof(DeltaX));
    double IDeltas.DeltaX => DeltaX ?? default;

    public double? DeltaY => GetDouble(nameof(DeltaY));
    double IDeltas.DeltaY => DeltaY ?? default;

    public double? DeltaZ => GetDouble(nameof(DeltaZ));
    double IDeltas.DeltaZ => DeltaZ ?? default;

    public long? Detail => GetLong(nameof(Detail));
    long IDetail.Detail => Detail ?? default;

    public float? ElapsedTime => GetFloat(nameof(ElapsedTime));
    float IAnimation.ElapsedTime => ElapsedTime ?? default;

    public DOMException? Error { get; private set; } = null; // TODO:
    DOMException IError.Error => Error ?? DOMException.Empty;

    public EventPhase? EventPhase => GetInt(nameof(EventPhase)) switch { int v => (EventPhase)v, _ => null };
    EventPhase IEvent.EventPhase => EventPhase ?? Web4.Events.EventPhase.None;

    public string? FileName => GetReference(nameof(FileName)) as string;
    string IError.FileName => FileName ?? string.Empty;

    public double? Gamma => GetDouble(nameof(Gamma));
    double IDeviceOrientation.Gamma => Gamma ?? default;

    public int? Height => GetInt(nameof(Height));
    int IWidthHeight.Height => Height ?? default;

    public string? InputType => GetReference(nameof(InputType)) as string;
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

    public double? Interval => GetDouble(nameof(Interval));
    double IDeviceMotion.Interval => Interval ?? default;

    public bool? IsComposing => GetBool(nameof(IsComposing));
    bool IIsComposing.IsComposing => IsComposing ?? default;

    public bool? IsPrimary => GetBool(nameof(IsPrimary));
    bool IPointer.IsPrimary => IsPrimary ?? default;

    public bool? IsTrusted => GetBool(nameof(IsTrusted));
    bool IEvent.IsTrusted => IsTrusted ?? default;

    public string? Key => GetReference(nameof(Key)) as string;
    string IKeys.Key => Key ?? string.Empty;

    public int? Length => GetInt(nameof(Length));
    int ILength.Length => Length ?? default;

    public bool? LengthComputable => GetBool(nameof(LengthComputable));
    bool IProgress.LengthComputable => LengthComputable ?? default;

    public int? LineNo => GetInt(nameof(LineNo));
    int IError.LineNo => LineNo ?? default;

    public long? Loaded => GetLong(nameof(Loaded));
    long IProgress.Loaded => Loaded ?? default;

    public KeyLocation? Location => GetInt(nameof(Location)) switch { int v => (KeyLocation)v, _ => null };
    KeyLocation IKeys.Location => Location ?? KeyLocation.Standard;

    public string? Message => GetReference(nameof(Message)) as string;
    string IError.Message => Message ?? string.Empty;

    public bool? MetaKey => GetBool(nameof(MetaKey));
    bool IModifierMeta.MetaKey => MetaKey ?? default;

    public double? MovementX => GetDouble(nameof(MovementX));
    double IMovementXY.MovementX => MovementX ?? default;

    public double? MovementY => GetDouble(nameof(MovementY));
    double IMovementXY.MovementY => MovementY ?? default;

    public string? NewState => GetReference(nameof(NewState)) as string;
    string IStates.NewState => NewState ?? string.Empty;

    public string? NewUrl => GetReference(nameof(NewUrl)) as string;
    string IHashChange.NewUrl => NewUrl ?? string.Empty;

    public double? OffsetX => GetDouble(nameof(OffsetX));
    double IOffsetXY.OffsetX => OffsetX ?? default;

    public double? OffsetY => GetDouble(nameof(OffsetY));
    double IOffsetXY.OffsetY => OffsetY ?? default;

    public string? OldState => GetReference(nameof(OldState)) as string;
    string IStates.OldState => OldState ?? string.Empty;

    public string? OldUrl => GetReference(nameof(OldUrl)) as string;
    string IHashChange.OldUrl => OldUrl ?? string.Empty;

    public double? PageX => GetDouble(nameof(PageX));
    double IPageXY.PageX => PageX ?? default;

    public double? PageY => GetDouble(nameof(PageY));
    double IPageXY.PageY => PageY ?? default;

    public bool? Persisted => GetBool(nameof(Persisted));
    bool IPersisted.Persisted => Persisted ?? default;

    public int? PointerID => GetInt(nameof(PointerID));
    int IPointer.PointerID => PointerID ?? default;

    public string? PointerType => GetReference(nameof(PointerType)) as string;
    string IPointer.PointerType => PointerType ?? string.Empty;

    public double? Pressure => GetDouble(nameof(Pressure));
    double IPressures.Pressure => Pressure ?? default;

    public string? PropertyName => GetReference(nameof(PropertyName)) as string;
    string IAnimation.PropertyName => PropertyName ?? string.Empty;

    public string? PseudoElement => GetReference(nameof(PseudoElement)) as string;
    string IAnimation.PseudoElement => PseudoElement ?? string.Empty;

    public EventTarget? RelatedTarget { get; private set; } = null; // TODO:
    EventTarget IRelatedTarget.RelatedTarget => RelatedTarget ?? EventTarget.Empty;

    public bool? Repeat => GetBool(nameof(Repeat));
    bool IKeys.Repeat => Repeat ?? default;

    public ABG? RotationRate { get; private set; } = null; // TODO:
    ABG IDeviceMotion.RotationRate => RotationRate ?? ABG.Empty;

    public double? ScreenX => GetDouble(nameof(ScreenX));
    double IScreenXY.ScreenX => ScreenX ?? default;

    public double? ScreenY => GetDouble(nameof(ScreenY));
    double IScreenXY.ScreenY => ScreenY ?? default;

    public bool? ShiftKey => GetBool(nameof(ShiftKey));
    bool IModifierShift.ShiftKey => ShiftKey ?? default;

    public bool? Skipped => GetBool(nameof(Skipped));
    bool ISkipped.Skipped => Skipped ?? default;

    
    public EventTarget? Submitter { get; private set; } = null; // TODO:
    EventTarget ISubmitter.Submitter => Submitter ?? EventTarget.Empty;

    public double? TangentialPressure => GetDouble(nameof(TangentialPressure));
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

    public double? TimeStamp => GetDouble(nameof(TimeStamp));
    double IEvent.TimeStamp => TimeStamp ?? default;

    public double? TiltX => GetDouble(nameof(TiltX));
    double ITilts.TiltX => TiltX ?? default;

    public double? TiltY => GetDouble(nameof(TiltY));
    double ITilts.TiltY => TiltY ?? default;

    public long? Total => GetLong(nameof(Total));
    long IProgress.Total => Total ?? default;

    public TouchPoint[]? Touches { get; private set; } = null; // TODO:
    TouchPoint[] ITouches.Touches => Touches ?? [];

    public double? Twist => GetDouble(nameof(Twist));
    double IPointer.Twist => Twist ?? default;

    public string? Type => GetReference(nameof(Type)) as string;
    string IEvent.Type => Type ?? string.Empty;

    public int? Width => GetInt(nameof(Width));
    int IWidthHeight.Width => Width ?? default;

    public double? X => GetDouble(nameof(X));
    double IXY.X => X ?? default;

    public double? Y => GetDouble(nameof(Y));
    double IXY.Y => Y ?? default;
}
