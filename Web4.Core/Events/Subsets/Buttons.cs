using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Buttons
        {
            const string Format = "button,buttons";

            /// <summary>
            /// The button number that was pressed (if applicable) when the mouse event was fired.
            /// </summary>
            Button Button { get; }

            /// <summary>
            /// The buttons being pressed (if any) when the mouse event was fired.
            /// </summary>
            ButtonFlag Buttons { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.Buttons> listener, 
        string? format = Event.Subsets.Buttons.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Buttons, Task> listener, 
        string? format = Event.Subsets.Buttons.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}