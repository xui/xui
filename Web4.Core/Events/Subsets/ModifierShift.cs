using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
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

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.ModifierShift> listener, 
        string? format = Events.Subsets.ModifierShift.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.ModifierShift, Task> listener, 
        string? format = Events.Subsets.ModifierShift.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}