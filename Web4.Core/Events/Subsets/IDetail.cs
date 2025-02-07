using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IDetail
        {
            const string Format = "detail";

            /// <summary>
            /// Returns a long with details about the event, depending on the event type.
            /// </summary>
            long Detail { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Detail> listener, 
            string? format = Detail.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Detail, Task> listener, 
            string? format = Detail.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}