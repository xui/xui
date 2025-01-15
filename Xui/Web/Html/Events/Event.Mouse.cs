namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial record class Event
{
    /// <summary>
    /// Event.Mouse is actually just an interface that Event implements explicitly.
    /// Casting Event to Event.Mouse or using it as a method parameter helps simplify
    /// your code and make the compiler happy since it swaps out Event's nullable properties 
    /// for non-nullable alternatives (e.g. `bool shiftKey` instead of `bool? shiftKey`).
    /// Sometimes Event can be a chore to work with in a static language since 
    /// Event must represents the union of all potential properties across all 
    /// event types due to JavaScript's dynamic nature.
    /// </summary>
    public interface Mouse : UI
    {
        // MouseEvent
        bool altKey { get; }
        int button { get; }
        int buttons { get; }
        double clientX { get; }
        double clientY { get; }
        bool ctrlKey { get; }
        bool metaKey { get; }
        double movementX { get; }
        double movementY { get; }
        double offsetX { get; }
        double offsetY { get; }
        double pageX { get; }
        double pageY { get; }
        HtmlElement relatedTarget { get; }
        double screenX { get; }
        double screenY { get; }
        bool shiftKey { get; }
        double x { get; }
        double y { get; }
    }

    bool Mouse.altKey => this.altKey ?? false;
    int Mouse.button => this.button ?? 0;
    int Mouse.buttons => this.buttons ?? 0;
    double Mouse.clientX => this.clientX ?? 0;
    double Mouse.clientY => this.clientY ?? 0;
    bool Mouse.ctrlKey => this.ctrlKey ?? false;
    bool Mouse.metaKey => this.metaKey ?? false;
    double Mouse.movementX => this.movementX ?? 0;
    double Mouse.movementY => this.movementX ?? 0;
    double Mouse.offsetX => this.offsetX ?? 0;
    double Mouse.offsetY => this.offsetY ?? 0;
    double Mouse.pageX => this.pageX ?? 0;
    double Mouse.pageY => this.pageY ?? 0;
    HtmlElement Mouse.relatedTarget => this.relatedTarget ?? new();
    double Mouse.screenX => this.screenX ?? 0;
    double Mouse.screenY => this.screenY ?? 0;
    bool Mouse.shiftKey => this.shiftKey ?? false;
    double Mouse.x => this.x ?? 0;
    double Mouse.y => this.y ?? 0;
}

#pragma warning restore IDE1006