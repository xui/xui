using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface IsComposing
                {
                    const string Format = "isComposing";

                    /// <summary>
                    /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
                    /// </summary>
                    bool IsComposing { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.IsComposing> listener, 
            string? format = Event.Subsets.IsComposing.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.IsComposing, Task> listener, 
            string? format = Event.Subsets.IsComposing.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}