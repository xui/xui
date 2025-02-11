namespace Web4.EventListeners;

public interface IClipboardListeners
{
    /// <summary>
    /// An event fired whenever the user initiates a copy action.
    /// </summary>
    Action<Event>? OnCopy { set; }
    
    /// <summary>
    /// An event fired whenever the user initiates a cut action.
    /// </summary>
    Action<Event>? OnCut { set; }
    
    /// <summary>
    /// An event fired whenever the user initiates a paste action.
    /// </summary>
    Action<Event>? OnPaste { set; }
    
}