namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial record class Event
{
    /// <summary>
    /// Event.UI is actually just an interface that Event implements explicitly.
    /// Casting Event to Event.UI or using it as a method parameter helps simplify
    /// your code and make the compiler happy since it swaps out Event's nullable properties 
    /// for non-nullable alternatives (e.g. `long detail` instead of `long? detail`).
    /// Sometimes Event can be a chore to work with in a static language since 
    /// Event must represents the union of all potential properties across all 
    /// event types due to JavaScript's dynamic nature.
    /// </summary>
    public interface UI
    {
        // UIEvent
        long detail { get; }
    }

    long UI.detail => this.detail ?? 0;
}

#pragma warning restore IDE1006