using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Wheel : UI, Mouse, Subsets.Deltas
    {
        new const string Format = 
            UI.Format + "," + 
            Mouse.Format + "," + 
            Subsets.Deltas.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Wheel> listener, 
        string? format = Events.Wheel.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Wheel, Task> listener, 
        string? format = Events.Wheel.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);
}