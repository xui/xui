using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
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

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.IsComposing> listener, 
        string? format = Events.Subsets.IsComposing.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.IsComposing, Task> listener, 
        string? format = Events.Subsets.IsComposing.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}