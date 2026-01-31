using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ISkipped : ISubset, IView
        {
            new const string Format = "skipped";

            /// <summary>
            /// Returns true if the user agent is skipping the element's rendering, or false otherwise.
            /// </summary>
            bool Skipped { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Skipped> listener, 
            string? format = Skipped.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Skipped, Task> listener, 
            string? format = Skipped.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}