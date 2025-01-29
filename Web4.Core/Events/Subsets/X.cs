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
        Action<Event.Subsets.X> listener, 
        string? format = Event.Subsets.X.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.X, Task> listener, 
        string? format = Event.Subsets.X.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}