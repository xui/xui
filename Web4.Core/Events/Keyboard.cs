using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing, Subsets.Keys
    {
        new const string Format = "code,key,location,repeat," + 
            UI.Format + "," + 
            Subsets.IsComposing.Format + "," +
            Subsets.Modifiers.Format + "," + 
            Subsets.Keys.Format;
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Keyboard> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Keyboard, Task> listener, 
        string? format = null, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}