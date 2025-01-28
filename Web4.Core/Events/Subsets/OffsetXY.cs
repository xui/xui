using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface OffsetXY
        {
            const string Format = "offsetX,offsetY";
            
            /// <summary>
            /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
            /// </summary>
            double OffsetX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
            /// </summary>
            double OffsetY { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.OffsetXY> listener, 
        string? format = Events.Subsets.OffsetXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.OffsetXY, Task> listener, 
        string? format = Events.Subsets.OffsetXY.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}