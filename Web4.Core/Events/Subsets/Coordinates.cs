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
        Action<Event.Subsets.Coordinates> listener, 
        string? format = Event.Subsets.Coordinates.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Coordinates, Task> listener, 
        string? format = Event.Subsets.Coordinates.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}