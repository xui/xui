using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IModifierCtrl
        {
            const string Format = "ctrlKey";

            /// <summary>
            /// Returns true if the control key was down when the event was fired.
            /// </summary>
            bool CtrlKey { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<ModifierCtrl> listener, 
            string? format = ModifierCtrl.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
                    
        public bool AppendFormatted(
            Func<ModifierCtrl, Task> listener, 
            string? format = ModifierCtrl.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}