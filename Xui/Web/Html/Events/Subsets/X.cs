using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface X
        {
            const string Format = "x";

            /// <summary>
            /// Alias for MouseEvent.clientX.
            /// </summary>
            double X { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.X> eventHandler, 
        string? format = Events.Subsets.X.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.X, Task> eventHandler, 
        string? format = Events.Subsets.X.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}