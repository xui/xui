using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Target<T> where T : unmanaged, IParsable<T>
        {
            const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget<T> Target { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.Target<int>> listener, 
        string? format = Event.Subsets.Target<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<int>, Task> listener, 
        string? format = Event.Subsets.Target<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target<DateTime>> listener, 
        string? format = Event.Subsets.Target<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Target<DateTime>, Task> listener, 
        string? format = Event.Subsets.Target<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);}