using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IIsComposingSubset
        {
            const string Format = "isComposing";

            /// <summary>
            /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
            /// </summary>
            bool IsComposing { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<IsComposing> listener, 
            string? format = IsComposing.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<IsComposing, Task> listener, 
            string? format = IsComposing.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}