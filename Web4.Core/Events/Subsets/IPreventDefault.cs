using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPreventDefault : ISubset
        {
            const string Format = "preventDefault";

            /// <summary>
            /// Cancels the event (if it is cancelable).
            /// </summary>
            void PreventDefault()
            {
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<PreventDefault> listener, 
            string? format = PreventDefault.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<PreventDefault, Task> listener, 
            string? format = PreventDefault.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Action<Event.PreventDefault> listener, 
            string? format = "preventDefault,null", 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.PreventDefault, Task> listener, 
            string? format = "preventDefault,null", 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}