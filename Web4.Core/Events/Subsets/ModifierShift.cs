using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface ModifierShift
                {
                    const string Format = "shiftKey";

                    /// <summary>
                    /// Returns true if the shift key was down when the event was fired.
                    /// </summary>
                    bool ShiftKey { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.ModifierShift> listener, 
            string? format = Event.Subsets.ModifierShift.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.ModifierShift, Task> listener, 
            string? format = Event.Subsets.ModifierShift.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}