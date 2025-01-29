using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Input<T> : UI, Subsets.Target<T>, Subsets.Data, Subsets.IsComposing
        where T : unmanaged, IParsable<T>
    {
        new const string Format = "dataTransfer,inputType," + 
            UI.Format + "," + 
            Subsets.Target<T>.Format + "," +
            Subsets.IsComposing.Format + "," +
            Subsets.Data.Format;
        
        /// <summary>
        /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
        /// </summary>
        DataTransfer DataTransfer { get; }

        /// <summary>
        /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
        /// </summary>
        string InputType { get; }

        new EventTarget<T> Target { get; }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Input<int>> listener, 
        string? format = Event.Input<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Input<int>, Task> listener, 
        string? format = Event.Input<int>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Input<DateTime>> listener, 
        string? format = Event.Input<DateTime>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Input<DateTime>, Task> listener, 
        string? format = Event.Input<DateTime>.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}