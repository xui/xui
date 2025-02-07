using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IRelatedTarget
        {
            const string Format = "relatedTarget";

            /// <summary>
            /// The secondary target for the event, if there is one.
            /// </summary>
            EventTarget RelatedTarget { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<RelatedTarget> listener, 
            string? format = RelatedTarget.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<RelatedTarget, Task> listener, 
            string? format = RelatedTarget.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}