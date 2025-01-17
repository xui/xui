namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial record class Event
{
    /// <summary>
    /// Event.Keyboard is actually just an interface that Event implements explicitly.
    /// Casting Event to Event.Keyboard or using it as a method parameter helps simplify
    /// your code and make the compiler happy since it swaps out Event's nullable properties 
    /// for non-nullable alternatives (e.g. `bool shiftKey` instead of `bool? shiftKey`).
    /// Sometimes Event can be a chore to work with in a static language since 
    /// Event must represents the union of all potential properties across all 
    /// event types due to JavaScript's dynamic nature.
    /// </summary>
    public interface Keyboard : UI
    {
        // KeyboardEvent
        bool altKey { get; }
        string code { get; }
        bool ctrlKey { get; }
        bool isComposing { get; }
        string key { get; }
        long location { get; }
        bool metaKey { get; }
        bool repeat { get; }
        bool shiftKey { get; }
    }

    bool Keyboard.altKey => this.altKey ?? default;
    string Keyboard.code => this.code ?? string.Empty;
    bool Keyboard.ctrlKey => this.ctrlKey ?? default;
    bool Keyboard.isComposing => this.isComposing ?? default;
    string Keyboard.key => this.key ?? string.Empty;
    long Keyboard.location => this.location ?? default;
    bool Keyboard.metaKey => this.metaKey ?? default;
    bool Keyboard.repeat => this.repeat ?? default;
    bool Keyboard.shiftKey => this.shiftKey ?? default;
}

#pragma warning restore IDE1006