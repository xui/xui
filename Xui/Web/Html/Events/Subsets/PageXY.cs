using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface PageXY
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
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.PageXY> eventHandler, 
        string? format = Events.Subsets.PageXY.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.PageXY, Task> eventHandler, 
        string? format = Events.Subsets.PageXY.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}