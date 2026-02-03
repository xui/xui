using System.Drawing;
using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface ITarget : 
            ISubset,
            Target<string>,
            Target<bool>,
            Target<int>,
            Target<long>,
            Target<float>,
            Target<double>,
            Target<decimal>,
            Target<DateTime>,
            Target<DateOnly>,
            Target<TimeOnly>,
            Target<Color>,
            Target<Uri>,
            IView
        {
            new const string TRIM = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            new EventTarget Target { get; }
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
    }
}