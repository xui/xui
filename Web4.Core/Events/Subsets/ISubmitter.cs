using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ISubmitter
        {
            const string Format = "submitter";

            /// <summary>
            /// An HTMLElement object which identifies the button or other element 
            /// which was invoked to trigger the form being submitted.
            /// </summary>
            EventTarget Submitter { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Submitter> listener, 
            string? format = Submitter.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Submitter, Task> listener, 
            string? format = Submitter.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}