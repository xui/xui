using System.Runtime.CompilerServices;

namespace Web4
{
    namespace Events
    {
        public partial interface OneLevelRemoved
        {
            public partial interface Subsets
            {
                public interface Data
                {
                    const string Format = "data";

                    /// <summary>
                    /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
                    /// </summary>
                    string Data { get; }
                }
            }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Event.Subsets.Data> listener, 
            string? format = Event.Subsets.Data.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Event.Subsets.Data, Task> listener, 
            string? format = Event.Subsets.Data.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}