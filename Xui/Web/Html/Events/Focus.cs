using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Focus : UI, Subsets.RelatedTarget
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.RelatedTarget.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Focus> eventHandler, 
        string? format = Events.Focus.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] 
        string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Focus, Task> eventHandler, 
        string? format = Events.Focus.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}