using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IDataTransferSubset
        {
            const string Format = "dataTransfer";

            /// <summary>
            /// Returns a DataTransfer object containing contextual information.
            /// </summary>
            Web4.DataTransfer DataTransfer { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.DataTransfer> listener, 
            string? format = Event.Subsets.DataTransfer.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.DataTransfer, Task> listener, 
            string? format = Event.Subsets.DataTransfer.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}