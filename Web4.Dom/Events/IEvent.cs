using System.Drawing;
using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom.Events;

public interface IEvent 
    : Target, CurrentTarget
{
    /// <summary>
    /// A boolean value indicating whether or not the event bubbles up through the DOM.
    /// </summary>
    bool Bubbles { get; }

    /// <summary>
    /// A boolean value indicating whether the event is cancelable.
    /// </summary>
    bool Cancelable { get; }
    
    /// <summary>
    /// A boolean indicating whether or not the event can bubble across the boundary 
    /// between the shadow DOM and the regular DOM.
    /// </summary>
    bool Composed { get; }

    /// <summary>
    /// Indicates whether or not the call to event.preventDefault() canceled the 
    /// event.
    /// </summary>
    bool DefaultPrevented { get; }

    /// <summary>
    /// Indicates which phase of the event flow is being processed. It is one of 
    /// the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    /// </summary>
    EventPhase EventPhase { get; }
    
    /// <summary>
    /// Indicates whether or not the event was initiated by the browser (after a 
    /// user click, for instance) or by a script (using an event creation method, 
    /// for example).
    /// </summary>
    bool IsTrusted { get; }
    
    /// <summary>
    /// The time at which the event was created (in milliseconds). By specification, 
    /// this value is time since epoch—but in reality, browsers' definitions vary. 
    /// In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
    /// </summary>
    double TimeStamp { get; }

    /// <summary>
    /// The name identifying the type of the event.
    /// </summary>
    string Type { get; }
}

public interface EventTarget<T>
{
    public string ID { get; }
    public string Name { get; }
    public string Type { get; }
    public bool? Checked { get; }
    public T Value { get; }
}

public record class EventTarget(
    string ID = "",
    string Name = "",
    string Type = "",
    bool? Checked = false,
    string Value = "") :
        EventTarget<string>,
        EventTarget<bool>,
        EventTarget<int>,
        EventTarget<long>,
        EventTarget<float>,
        EventTarget<double>,
        EventTarget<decimal>,
        EventTarget<DateTime>,
        EventTarget<DateOnly>,
        EventTarget<TimeOnly>,
        EventTarget<Color>,
        EventTarget<Uri>
{
    public static readonly EventTarget Empty = new();

    public bool? ValueAsBool => Checked;
    public int? ValueAsInt => int.TryParse(Value, out var i) ? i : null;
    public long? ValueAsLong => long.TryParse(Value, out var i) ? i : null;
    public float? ValueAsFloat => float.TryParse(Value, out var f) ? f : null;
    public double? ValueAsDouble => double.TryParse(Value, out var d) ? d : null;
    public decimal? ValueAsDecimal => decimal.TryParse(Value, out var m) ? m : null;
    public DateTime? ValueAsDateTime => DateTime.TryParse(Value, out var d) ? d : null;
    public DateOnly? ValueAsDateOnly => DateOnly.TryParse(Value, out var d) ? d : null;
    public TimeOnly? ValueAsTimeOnly => TimeOnly.TryParse(Value, out var t) ? t : null;
    public Color? ValueAsColor => Value != null ? ColorTranslator.FromHtml(Value) : null; // TODO: Should "TryParse" not throw.
    public Uri? ValueAsUri => Uri.TryCreate(Value, UriKind.RelativeOrAbsolute, out var u) ? u : null;

    string EventTarget<string>.Value => Value ?? string.Empty;
    bool EventTarget<bool>.Value => ValueAsBool ?? default;
    int EventTarget<int>.Value => ValueAsInt ?? default;
    long EventTarget<long>.Value => ValueAsLong ?? default;
    float EventTarget<float>.Value => ValueAsFloat ?? default;
    double EventTarget<double>.Value => ValueAsDouble ?? default;
    decimal EventTarget<decimal>.Value => ValueAsDecimal ?? default;
    DateTime EventTarget<DateTime>.Value => ValueAsDateTime ?? default;
    DateOnly EventTarget<DateOnly>.Value => ValueAsDateOnly ?? default;
    TimeOnly EventTarget<TimeOnly>.Value => ValueAsTimeOnly ?? default;
    Color EventTarget<Color>.Value => ValueAsColor ?? default;
    Uri EventTarget<Uri>.Value => ValueAsUri ?? new Uri("about:blank");
}