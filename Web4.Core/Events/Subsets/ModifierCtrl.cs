using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
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
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.ModifierCtrl> listener, 
            string? format = Event.Subsets.ModifierCtrl.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
                    
        public bool AppendFormatted(
            Func<Event.Subsets.ModifierCtrl, Task> listener, 
            string? format = Event.Subsets.ModifierCtrl.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}