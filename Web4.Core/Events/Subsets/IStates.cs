using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IStates : ISubset
        {
            const string Format = "newState,oldState";

            /// <summary>
            /// A string (either "open" or "closed"), representing the state the element is transitioning to.
            /// </summary>
            string NewState { get; }

            /// <summary>
            /// A string (either "open" or "closed"), representing the state the element is transitioning from.
            /// </summary>
            string OldState { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<States> listener, 
            string? format = States.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<States, Task> listener, 
            string? format = States.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}