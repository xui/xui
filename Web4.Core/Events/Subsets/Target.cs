using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface Target : 
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
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.Target> listener, 
            string? format = Event.Subsets.Target.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.Target, Task> listener, 
            string? format = Event.Subsets.Target.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}