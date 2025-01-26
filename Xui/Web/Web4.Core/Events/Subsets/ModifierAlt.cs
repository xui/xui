using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
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

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.ModifierAlt> eventHandler, 
        string? format = Events.Subsets.ModifierAlt.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.ModifierAlt, Task> eventHandler, 
        string? format = Events.Subsets.ModifierAlt.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}