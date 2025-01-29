using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Target
        {
            const string Format = "target";

            /// <summary>
            /// A reference to the object to which the event was originally dispatched.
            /// </summary>
            EventTarget Target { get; }
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