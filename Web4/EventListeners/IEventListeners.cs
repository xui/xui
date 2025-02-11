namespace Web4.EventListeners;

public interface IEventListeners
{
    /// <summary>
    /// Fired when the value of an input element is about to be modified.
    /// </summary>
    Action<Event>? OnBeforeInput { set; }

    /// <summary>
    /// Fires on any element with content-visibility: auto set on it when it starts or stops being relevant to the user and skipping its contents.
    /// </summary>
    Action<Event>? OnContentVisibilityAutoStateChange { set; }

    /// <summary>
    /// Fires when an element's value is changed as a direct result of a user action.
    /// </summary>
    Action<Event>? OnInput { set; }

    /// <summary>
    /// Fired when a Content Security Policy is violated.
    /// </summary>
    Action<Event>? OnSecurityPolicyViolation { set; }

}