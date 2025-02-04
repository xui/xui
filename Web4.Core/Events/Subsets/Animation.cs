using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Animation
        {
            const string Format = "animationName,elapsedTime,PseudoElement";

            /// <summary>
            /// A string containing the value of the animation-name that generated the animation.
            /// </summary>
            string AnimationName { get; }

            /// <summary>
            /// A float giving the amount of time the animation has been running, in seconds, 
            /// when this event fired, excluding any time the animation was paused. 
            /// For an animationstart event, elapsedTime is 0.0 unless there was a negative 
            /// value for animation-delay, in which case the event will be fired with 
            /// elapsedTime containing (-1 * delay).
            /// </summary>
            float ElapsedTime { get; }

            /// <summary>
            /// A string, starting with '::', containing the name of the pseudo-element the 
            /// animation runs on. If the animation doesn't run on a pseudo-element but 
            /// on the element, an empty string: ''.
            /// </summary>
            string PseudoElement { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.Animation> listener, 
        string? format = Event.Subsets.Animation.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Animation, Task> listener, 
        string? format = Event.Subsets.Animation.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}