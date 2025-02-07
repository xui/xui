using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IScreenXY
        {
            const string Format = "screenX,screenY";

            /// <summary>
            /// The X coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double ScreenX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double ScreenY { get; }
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<ScreenXY> listener, 
            string? format = ScreenXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<ScreenXY, Task> listener, 
            string? format = ScreenXY.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}