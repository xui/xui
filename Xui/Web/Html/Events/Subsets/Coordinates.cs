using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Coordinates : XY, ClientXY, MovementXY, OffsetXY, PageXY, ScreenXY
        {
            new const string Format = 
                XY.Format + "," + 
                ClientXY.Format + "," + 
                MovementXY.Format + "," + 
                OffsetXY.Format + "," + 
                PageXY.Format + "," + 
                ScreenXY.Format;
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.Coordinates> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.Coordinates.Format, 
                expression);

    public bool AppendFormatted(
        Func<Events.Subsets.Coordinates, Task> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.Coordinates.Format, 
                expression);
}