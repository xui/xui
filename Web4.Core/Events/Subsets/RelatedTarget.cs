using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface RelatedTarget
        {
            const string Format = "relatedTarget";

            /// <summary>
            /// The secondary target for the event, if there is one.
            /// </summary>
            EventTarget RelatedTarget { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.RelatedTarget> listener, 
        string? format = Event.Subsets.RelatedTarget.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.RelatedTarget, Task> listener, 
        string? format = Event.Subsets.RelatedTarget.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}