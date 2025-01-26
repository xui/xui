using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface ModifierCtrl
        {
            const string Format = "ctrlKey";

            /// <summary>
            /// Returns true if the control key was down when the event was fired.
            /// </summary>
            bool CtrlKey { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.ModifierCtrl> eventHandler, 
        string? format = Events.Subsets.ModifierCtrl.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
                
    public bool AppendFormatted(
        Func<Events.Subsets.ModifierCtrl, Task> eventHandler, 
        string? format = Events.Subsets.ModifierCtrl.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}