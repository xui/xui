using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface ModifierMeta
                {
                    const string Format = "metaKey";

                    /// <summary>
                    /// Returns true if the meta key was down when the event was fired.
                    /// </summary>
                    bool MetaKey { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.ModifierMeta> listener, 
            string? format = Event.Subsets.ModifierMeta.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.ModifierMeta, Task> listener, 
            string? format = Event.Subsets.ModifierMeta.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}