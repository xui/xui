using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface ModifierAlt
                {
                    const string Format = "altKey";

                    /// <summary>
                    /// Returns true if the alt key was down when the event was fired.
                    /// </summary>
                    bool AltKey { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.ModifierAlt> listener, 
            string? format = Event.Subsets.ModifierAlt.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.ModifierAlt, Task> listener, 
            string? format = Event.Subsets.ModifierAlt.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}