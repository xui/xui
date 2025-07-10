using System.Drawing;
using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ITarget<T> : ISubset
        {
            const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget<T> Target { get; }
        }

        public interface EventTarget<T>
        {
            public string ID { get; }
            public string Name { get; }
            public string Type { get; }
            public bool? Checked { get; }
            public T Value { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Target<string>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Target<bool>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Target<int>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        // TODO: I think converting `Func<Event, T>` to `Action<Event>` (e => listener(e)) is allocating.  
        // Verify this and fix it.  There are 4 below, and one more `e => c++` at the bottom.

        // How interesting.  Long, float, double, and decimal must use the signature
        // `Func<Target<T>, T>` instead of `Action<Target<T>>` or else the 
        // "call is ambiguous" due to these types' ability to cast to other types.
        public bool AppendFormatted(
            Func<Target<long>, long> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(e => listener(e), format, expression);

        public bool AppendFormatted(
            Func<Target<float>, float> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(e => listener(e), format, expression);

        public bool AppendFormatted(
            Func<Target<double>, double> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(e => listener(e), format, expression);

        public bool AppendFormatted(
            Func<Target<decimal>, decimal> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(e => listener(e), format, expression);

        public bool AppendFormatted(
            Action<Target<DateTime>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Target<DateOnly>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Target<TimeOnly>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Target<Color>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Target<Uri>> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(listener, format, expression);

        // This is the only one where listener is a Func.  Also, `int` is the only return type  
        // we need to support because because I want `onclick={e => c++}` to work without extra brackets.
        public bool AppendFormatted(
            Func<Target<int>, int> listener,
            string? format = Target.Format,
            [CallerArgumentExpression(nameof(listener))] string? expression = null)
                => AppendEventListener(e => listener(e), format, expression);
    }
}