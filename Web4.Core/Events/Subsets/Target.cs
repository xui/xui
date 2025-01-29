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
        Action<Events.Subsets.Target> listener, 
        string? format = Events.Subsets.Target.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.Target, Task> listener, 
        string? format = Events.Subsets.Target.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}