using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IXSubset
        {
            const string Format = "x";

            /// <summary>
            /// Alias for MouseEvent.clientX.
            /// </summary>
            double X { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<X> listener, 
            string? format = X.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<X, Task> listener, 
            string? format = X.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}