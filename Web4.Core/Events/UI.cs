using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface UI: Base
    {
        /// <summary>
        /// Returns a long with details about the event, depending on the event type.
        /// </summary>
        long Detail { get; }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.UI> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.UI, Task> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}