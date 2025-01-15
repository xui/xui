namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial record class Event
{
    /// <summary>
    /// Event.Wheel is actually just an interface that Event implements explicitly.
    /// Casting Event to Event.Wheel or using it as a method parameter helps simplify
    /// your code and make the compiler happy since it swaps out Event's nullable properties 
    /// for non-nullable alternatives (e.g. `bool shiftKey` instead of `bool? shiftKey`).
    /// Sometimes Event can be a chore to work with in a static language since 
    /// Event must represents the union of all potential properties across all 
    /// event types due to JavaScript's dynamic nature.
    /// </summary>
    public interface Wheel : UI
    {
        // WheelEvent
        double deltaX { get; }
        double deltaY { get; }
        double deltaZ { get; }
        long deltaMode { get; }
    }

    double Wheel.deltaX => this.deltaX ?? 0;
    double Wheel.deltaY => this.deltaY ?? 0;
    double Wheel.deltaZ => this.deltaZ ?? 0;
    long Wheel.deltaMode => this.deltaMode ?? 0;
}

#pragma warning restore IDE1006