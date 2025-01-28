using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Input
    {
        public interface Int : UI, Subsets.Data, Subsets.IsComposing
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

            new IntEventTarget Target { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Input.Int> eventHandler, 
        string? format = Events.Input.Int.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Input.Int, Task> eventHandler, 
        string? format = Events.Input.Int.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}