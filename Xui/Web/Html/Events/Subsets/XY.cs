using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface XY : X, Y
        {
            new const string Format = Subsets.X.Format + "," + Subsets.Y.Format;
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.XY> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.XY.Format, 
                expression);

    public bool AppendFormatted(
        Func<Events.Subsets.XY, Task> eventHandler, 
        string? format = null, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(
                eventHandler, 
                format ?? Events.Subsets.XY.Format, 
                expression);
}