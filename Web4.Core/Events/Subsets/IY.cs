using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IY
        {
            const string Format = "y";

            /// <summary>
            /// Alias for MouseEvent.clientY.
            /// </summary>
            double Y { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Y> listener, 
            string? format = Y.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Y, Task> listener, 
            string? format = Y.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}