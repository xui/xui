namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial record class Event
{
    /// <summary>
    /// Event.Touch is actually just an interface that Event implements explicitly.
    /// Casting Event to Event.Touch or using it as a method parameter helps simplify
    /// your code and make the compiler happy since it swaps out Event's nullable properties 
    /// for non-nullable alternatives (e.g. `bool shiftKey` instead of `bool? shiftKey`).
    /// Sometimes Event can be a chore to work with in a static language since 
    /// Event must represents the union of all potential properties across all 
    /// event types due to JavaScript's dynamic nature.
    /// </summary>
    public interface Touch : UI
    {
        // TouchEvent
        bool altKey { get; }
        TouchPoint[] changedTouches { get; }
        bool ctrlKey { get; }
        bool metaKey { get; }
        bool shiftKey { get; }
        TouchPoint[] targetTouches { get; }
        TouchPoint[] touches { get; }

    }

    bool Touch.altKey => this.altKey ?? false;
    TouchPoint[] Touch.changedTouches => this.changedTouches ?? [];
    bool Touch.ctrlKey => this.ctrlKey ?? false;
    bool Touch.metaKey => this.metaKey ?? false;
    bool Touch.shiftKey => this.shiftKey ?? false;
    TouchPoint[] Touch.targetTouches => this.targetTouches ?? [];
    TouchPoint[] Touch.touches => this.touches ?? [];
}

#pragma warning restore IDE1006