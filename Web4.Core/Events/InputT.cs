using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public interface Input<T> : UI, Subsets.Data, Subsets.IsComposing
        where T : unmanaged, IParsable<T>
    {
        new const string Format = "dataTransfer,inputType," + 
            UI.Format + "," + 
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
        Action<Events.Input<int>> eventHandler, 
        string? format = Events.Input<int>.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Input<int>, Task> eventHandler, 
        string? format = Events.Input<int>.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Action<Events.Input<DateTime>> eventHandler, 
        string? format = Events.Input<int>.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Input<DateTime>, Task> eventHandler, 
        string? format = Events.Input<int>.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}