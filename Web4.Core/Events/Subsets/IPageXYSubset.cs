using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IPageXYSubset
        {
            const string Format = "pageX,pageY";

            /// <summary>
            /// The X coordinate of the mouse pointer relative to the whole document.
            /// </summary>
            double PageX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the whole document.
            /// </summary>
            double PageY { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<PageXY> listener, 
            string? format = PageXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<PageXY, Task> listener, 
            string? format = PageXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}