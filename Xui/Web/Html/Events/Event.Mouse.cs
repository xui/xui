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

    bool Mouse.altKey => this.altKey ?? default;
    int Mouse.button => this.button ?? default;
    int Mouse.buttons => this.buttons ?? default;
    double Mouse.clientX => this.clientX ?? default;
    double Mouse.clientY => this.clientY ?? default;
    bool Mouse.ctrlKey => this.ctrlKey ?? default;
    bool Mouse.metaKey => this.metaKey ?? default;
    double Mouse.movementX => this.movementX ?? default;
    double Mouse.movementY => this.movementX ?? default;
    double Mouse.offsetX => this.offsetX ?? default;
    double Mouse.offsetY => this.offsetY ?? default;
    double Mouse.pageX => this.pageX ?? default;
    double Mouse.pageY => this.pageY ?? default;
    HtmlElement Mouse.relatedTarget => this.relatedTarget ?? HtmlElement.Empty;
    double Mouse.screenX => this.screenX ?? default;
    double Mouse.screenY => this.screenY ?? default;
    bool Mouse.shiftKey => this.shiftKey ?? default;
    double Mouse.x => this.x ?? default;
    double Mouse.y => this.y ?? default;
}

#pragma warning restore IDE1006