using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IError
        {
            const string Format = "error";

            string Message { get; }
            string FileName { get; }
            int LineNo { get; }
            int ColNo { get; }
            DOMException Error { get; }
        }

        public record struct DOMException(
            string Name = "",
            string Message = ""
        ) {
            public static readonly DOMException Empty = new();
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Error> listener, 
            string? format = Error.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Error, Task> listener, 
            string? format = Error.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}