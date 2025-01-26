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
        Action<Events.Subsets.RelatedTarget> eventHandler, 
        string? format = Events.Subsets.RelatedTarget.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.RelatedTarget, Task> eventHandler, 
        string? format = Events.Subsets.RelatedTarget.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}