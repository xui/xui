using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ICurrentTarget : ISubset, IView
        {
            new const string Format = "currentTarget";

            /// <summary>
            /// A reference to the currently registered target for the event. This is the 
            /// object to which the event is currently slated to be sent. It's possible 
            /// this has been changed along the way through retargeting.
            /// </summary>
            EventTarget CurrentTarget { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<CurrentTarget> listener, 
            string? format = CurrentTarget.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<CurrentTarget, Task> listener, 
            string? format = CurrentTarget.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}