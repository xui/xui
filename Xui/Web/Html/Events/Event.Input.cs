namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial record class Event
{
    /// <summary>
    /// Event.Input is actually just an interface that Event implements explicitly.
    /// Casting Event to Event.Input or using it as a method parameter helps simplify
    /// your code and make the compiler happy since it swaps out Event's nullable properties 
    /// for non-nullable alternatives (e.g. `string data` instead of `string? data`).
    /// Sometimes Event can be a chore to work with in a static language since 
    /// Event must represents the union of all potential properties across all 
    /// event types due to JavaScript's dynamic nature.
    /// </summary>
    public interface Input : UI
    {
        // WheelEvent
        string data { get; }
        DataTransfer dataTransfer { get; }
        string inputType { get; }
        bool isComposing { get; }
    }

    string Input.data => this.data ?? string.Empty;
    DataTransfer Input.dataTransfer => this.dataTransfer ?? new();
    string Input.inputType => this.inputType ?? string.Empty;
    bool Input.isComposing => this.isComposing ?? false;
}

#pragma warning restore IDE1006