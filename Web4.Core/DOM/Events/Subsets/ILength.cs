using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface ILength : ISubset, IView
        {
            new const string Format = "length";

            /// <summary>
            /// Returns an integer representing the number of data items stored in the Storage object.
            /// </summary>
            int Length { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Length> listener, 
            string? format = Event.Subsets.Length.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Length, Task> listener, 
            string? format = Event.Subsets.Length.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}