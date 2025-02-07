using System.Drawing;
using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ITargetSubset : 
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
            Target<Uri>
        {
            new const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            new EventTarget Target { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Target> listener, 
            string? format = Target.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Target, Task> listener, 
            string? format = Target.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}