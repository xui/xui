using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface HashChange
        {
            const string Format = "newUrl,oldUrl";

            /// <summary>
            /// The new URL to which the window is navigating.
            /// </summary>
            string NewUrl { get; }

            /// <summary>
            /// The previous URL from which the window was navigated.
            /// </summary>
            string OldUrl { get; }

        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.HashChange> listener, 
        string? format = Event.Subsets.HashChange.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.HashChange, Task> listener, 
        string? format = Event.Subsets.HashChange.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}