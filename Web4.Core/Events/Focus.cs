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
        Action<Event.Focus> listener, 
        string? format = Event.Focus.Format, 
        [CallerArgumentExpression(nameof(listener))] 
        string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Focus, Task> listener, 
        string? format = Event.Focus.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}