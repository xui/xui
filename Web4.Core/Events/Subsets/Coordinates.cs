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
        Action<Events.Subsets.Coordinates> listener, 
        string? format = Events.Subsets.Coordinates.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.Coordinates, Task> listener, 
        string? format = Events.Subsets.Coordinates.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);
}