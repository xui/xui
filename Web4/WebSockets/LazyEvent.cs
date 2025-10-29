
using System.Buffers;
using System.Drawing;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Web4.Core.DOM;
using Web4.Events;
using Web4.Events.Subsets;

namespace Web4.WebSockets;

/// <summary>
/// A WebSocket-optimized implementation of the Event interface.
/// Implementing as a regular POCO would be rather large from a memory standpoint 
/// since there are so many potential properties to cover, 
/// so this takes inspiration from JavaScript-style objects
/// by using a dictionary as the backing store in conjunction with lazy-reads from 
/// the ReadOnlySequence<byte> that came from the network.
/// Additionally, if the only properties lazily accessed are value types (e.g. bool, int, long, double)
/// then this struct will avoid allocating a surplus of memory on the heap by pooling its dictionary.
/// </summary>
public record struct LazyEvent : Event, IDisposable
{
    private static readonly ObjectPool<Dictionary<string, long>> valueDictionaryPool = ObjectPool.Create<Dictionary<string, long>>();

    private readonly ReadOnlySequence<byte> rpcMessage;
    private readonly ReadOnlySequence<byte> eventParam;
    private readonly WebSocketTransport transport;
    private Dictionary<string, long>? values = null; // Here, longs are used to encode bools, ints, and doubles.
    private Dictionary<string, object>? references = null;

    public LazyEvent(ReadOnlySequence<byte> rpcMessage, ReadOnlySequence<byte> eventParam, WebSocketTransport transport)
    {
        this.rpcMessage = rpcMessage;
        this.eventParam = eventParam;
        this.transport = transport;
    }

    public void Dispose()
    {
        values?.Clear();
        references?.Clear();
        if (values is not null)
            valueDictionaryPool.Return(values);
        rpcMessage.ReturnToPool();
    }

    private void LazyParse(bool canIgnoreRefTypes = false)
    {
        if (values is null)
        {
            Parse(canIgnoreRefTypes);
        }

        if (!canIgnoreRefTypes && references is null)
        {
            Parse(canIgnoreRefTypes);
        }
    }

    private void Parse(bool canIgnoreRefTypes = false)
    {
        try
        {
            var reader = new Utf8JsonReader(eventParam);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.HasValueSequence
                        ? Keymaker.GetKeyIfCached(reader.ValueSequence)
                        : Keymaker.GetKeyIfCached(reader.ValueSpan);

                    if (propertyName is not null)
                    {
                        reader.Read();
                        if (types.TryGetValue(propertyName, out var type))
                        {
                            long? value = type switch
                            {
                                _ when type == typeof(bool) => reader.GetBoolean() ? 1 : 0,
                                _ when type == typeof(int) => reader.GetInt32(),
                                _ when type == typeof(long) => reader.GetInt64(),
                                _ when type == typeof(double) => BitConverter.DoubleToInt64Bits(reader.GetDouble()),
                                _ => null,
                            };
                            if (value is long l)
                            {
                                values ??= valueDictionaryPool.Get();
                                values[propertyName] = l;
                            }

                            if (!canIgnoreRefTypes)
                            {
                                if (type == typeof(string))
                                {
                                    references ??= [];
                                    var str = reader.GetString();
                                    if (str is not null)
                                        references[propertyName] = str;
                                }
                                else if (type == typeof(ABG))
                                {
                                    references ??= [];
                                    // TODO: Implement
                                }
                                else if (type == typeof(DataTransferContainer))
                                {
                                    references ??= [];
                                    // TODO: Implement
                                }
                                else if (type == typeof(DOMException))
                                {
                                    references ??= [];
                                    // TODO: Implement
                                }
                                else if (type == typeof(EventTarget))
                                {
                                    references ??= [];
                                    ParseEventTarget(reader, propertyName);
                                }
                                else if (type == typeof(TouchPoint[]))
                                {
                                    references ??= [];
                                    // TODO: Implement
                                }
                                else if (type == typeof(XYZ))
                                {
                                    references ??= [];
                                    // TODO: Implement
                                }
                            }
                        }
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
        LazyParse(canIgnoreRefTypes: false);
        return references?.GetValueOrDefault(propName);
    }

    private bool? GetBool(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return values is null
            ? null
            : values.TryGetValue(propName, out long value) ? value != 0 : null;
    }

    private int? GetInt(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return values is null
            ? null
            : values.TryGetValue(propName, out long value) ? (int)value : null;
    }

    private long? GetLong(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return values is null
            ? null
            : values.TryGetValue(propName, out long value) ? value : null;
    }

    private double? GetDouble(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return values is null
            ? null
            : values.TryGetValue(propName, out long value)
                ? BitConverter.Int64BitsToDouble(value)
                : null;
    }

    private EventTarget? GetTarget(string propName)
    {
        LazyParse(canIgnoreRefTypes: false);
        return references?.GetValueOrDefault(propName) as EventTarget;
    }

    private void ParseEventTarget(Utf8JsonReader reader, string propertyName)
    {
        string id = "", type = "", value = "", name = "";
        bool @checked = false;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                references![propertyName] = new EventTarget(id, name, type, @checked, value);
                return;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                _ = ParseString(reader, "id", ref id) ||
                    ParseString(reader, "type", ref type) ||
                    ParseString(reader, "value", ref value) ||
                    ParseString(reader, "name", ref name) ||
                    ParseBool(reader, "checked", ref @checked);
            }
        }
    }

    private bool ParseString(Utf8JsonReader reader, string propertyName, ref string value)
    {
        if (reader.ValueTextEquals(propertyName))
        {
            references ??= [];
            reader.Read();
            value = reader.GetString() ?? "";
            return true;
        }
        return false;
    }

    private bool ParseBool(Utf8JsonReader reader, string propertyName, ref bool value)
    {
        if (reader.ValueTextEquals(propertyName))
        {
            references ??= [];
            reader.Read();
            value = reader.GetBoolean();
            return true;
        }
        return false;
    }

    static LazyEvent()
    {
        foreach (var key in types.Keys)
            Keymaker.CacheKey(key);
    }

    private static readonly Dictionary<string, Type> types = new()
    {
        ["absolute"] = typeof(bool),
        ["acceleration"] = typeof(XYZ),
        ["accelerationIncludingGravity"] = typeof(XYZ),
        ["alpha"] = typeof(double),
        ["altitudeAngle"] = typeof(double),
        ["altKey"] = typeof(bool),
        ["animationName"] = typeof(string),
        ["azimuthAngle"] = typeof(double),
        ["beta"] = typeof(double),
        ["bubbles"] = typeof(bool),
        ["button"] = typeof(int),
        ["buttons"] = typeof(int),
        ["cancelable"] = typeof(bool),
        ["changedTouches"] = typeof(TouchPoint[]),
        ["clientX"] = typeof(double),
        ["clientY"] = typeof(double),
        ["code"] = typeof(string),
        ["colNo"] = typeof(int),
        ["composed"] = typeof(bool),
        ["ctrlKey"] = typeof(bool),
        ["currentTarget"] = typeof(EventTarget),
        ["data"] = typeof(string),
        ["dataTransfer"] = typeof(DataTransferContainer),
        ["defaultPrevented"] = typeof(bool),
        ["deltaMode"] = typeof(int),
        ["deltaX"] = typeof(double),
        ["deltaY"] = typeof(double),
        ["deltaZ"] = typeof(double),
        ["detail"] = typeof(long),
        ["elapsedTime"] = typeof(double),
        ["error"] = typeof(DOMException),
        ["eventPhase"] = typeof(int),
        ["fileName"] = typeof(string),
        ["gamma"] = typeof(double),
        ["height"] = typeof(int),
        ["inputType"] = typeof(string),
        ["interval"] = typeof(double),
        ["isComposing"] = typeof(bool),
        ["isPrimary"] = typeof(bool),
        ["isTrusted"] = typeof(bool),
        ["key"] = typeof(string),
        ["length"] = typeof(int),
        ["lengthComputable"] = typeof(bool),
        ["lineNo"] = typeof(int),
        ["loaded"] = typeof(long),
        ["location"] = typeof(int),
        ["message"] = typeof(string),
        ["metaKey"] = typeof(bool),
        ["movementX"] = typeof(double),
        ["movementY"] = typeof(double),
        ["newState"] = typeof(string),
        ["newUrl"] = typeof(string),
        ["offsetX"] = typeof(double),
        ["offsetY"] = typeof(double),
        ["oldState"] = typeof(string),
        ["oldUrl"] = typeof(string),
        ["pageX"] = typeof(double),
        ["pageY"] = typeof(double),
        ["persisted"] = typeof(bool),
        ["pointerID"] = typeof(int),
        ["pointerType"] = typeof(string),
        ["pressure"] = typeof(double),
        ["propertyName"] = typeof(string),
        ["pseudoElement"] = typeof(string),
        ["relatedTarget"] = typeof(EventTarget),
        ["repeat"] = typeof(bool),
        ["rotationRate"] = typeof(ABG),
        ["screenX"] = typeof(double),
        ["screenY"] = typeof(double),
        ["shiftKey"] = typeof(bool),
        ["skipped"] = typeof(bool),
        ["submitter"] = typeof(EventTarget),
        ["tangentialPressure"] = typeof(double),
        ["target"] = typeof(EventTarget),
        ["targetTouches"] = typeof(TouchPoint[]),
        ["timeStamp"] = typeof(double),
        ["tiltX"] = typeof(double),
        ["tiltY"] = typeof(double),
        ["total"] = typeof(long),
        ["touches"] = typeof(TouchPoint[]),
        ["twist"] = typeof(double),
        ["type"] = typeof(string),
        ["width"] = typeof(int),
        ["x"] = typeof(double),
        ["y"] = typeof(double),
        ["_id"] = typeof(int)
    };

    private bool PrintMembers(StringBuilder stringBuilder)
    {
        bool isFirst = true;
        if (values is not null)
        {
            foreach (var pair in values)
            {
                if (isFirst)
                    isFirst = false;
                else
                    stringBuilder.Append(", ");

                if (types.TryGetValue(pair.Key, out var type))
                {
                    if (type == typeof(bool))
                        stringBuilder.Append($"{pair.Key}: {GetBool(pair.Key) switch { true => "true", false => "false", _ => "null" }}");
                    else if (type == typeof(int))
                        stringBuilder.Append($"{pair.Key}: {GetInt(pair.Key)}");
                    else if (type == typeof(long))
                        stringBuilder.Append($"{pair.Key}: {GetLong(pair.Key)}");
                    else if (type == typeof(double))
                        stringBuilder.Append($"{pair.Key}: {GetDouble(pair.Key)}");
                }
            }
        }

        if (references is not null)
        {
            foreach (var pair in references)
            {
                if (types.TryGetValue(pair.Key, out var type))
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        stringBuilder.Append(", ");

                    if (type == typeof(string))
                        stringBuilder.Append($"{pair.Key}: \"{GetReference(pair.Key)}\"");
                    // TODO: Implement others
                }
            }
        }
        return true;
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

    public EventTarget? CurrentTarget => GetTarget("currentTarget");
    EventTarget ICurrentTarget.CurrentTarget => CurrentTarget ?? EventTarget.Empty;

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

    public EventTarget? RelatedTarget => GetTarget("relatedTarget");
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

    public EventTarget? Submitter => GetTarget("submitter");
    EventTarget ISubmitter.Submitter => Submitter ?? EventTarget.Empty;

    public double? TangentialPressure => GetDouble("tangentialPressure");
    double IPressures.TangentialPressure => TangentialPressure ?? default;

    public EventTarget? Target => GetTarget("target");
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

    public IWindow? View => this.transport;
    IWindow IView.View => View!;

    public int? Width => GetInt("width");
    int IWidthHeight.Width => Width ?? default;

    public double? X => GetDouble("x");
    double IXY.X => X ?? default;

    public double? Y => GetDouble("y");
    double IXY.Y => Y ?? default;

    public void StopPropagation() => transport.Propagation.Stop();

    public void StopImmediatePropagation() => transport.Propagation.StopImmediate();
}
