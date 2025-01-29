using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing
    {
        new const string Format = "code,key,location,repeat," + 
            UI.Format + "," + 
            Subsets.IsComposing.Format + "," +
            Subsets.Modifiers.Format;
        
        /// <summary>
        /// Returns a string with the code value of the physical key represented by the event.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Returns a string representing the key value of the key represented by the event.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Returns a number representing the location of the key on the keyboard or other input device.
        /// </summary>
        KeyLocation Location { get; }

        /// <summary>
        /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
        /// </summary>
        bool Repeat { get; }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Keyboard> listener, 
        string? format = Event.Keyboard.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Keyboard, Task> listener, 
        string? format = Event.Keyboard.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}