using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
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

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.Data> listener, 
        string? format = Events.Subsets.Data.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.Data, Task> listener, 
        string? format = Events.Subsets.Data.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}