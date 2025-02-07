using static Web4.Events.Aliases.Subsets;

namespace Web4.Events;

public interface IAnimationEvent            : IBaseEvent, Animation { }
public interface ICompositionEvent          : IUIEvent, Data { }
public interface IDeviceMotionEvent         : IBaseEvent, DeviceMotion { }
public interface IDeviceOrientationEvent    : IBaseEvent, DeviceOrientation { }
public interface IDragEvent                 : IMouseEvent, Event.Subsets.DataTransfer { }
public interface IFocusEvent                : IUIEvent, RelatedTarget { }
public interface IHashChangeEvent           : IBaseEvent, HashChange { }
public interface IKeyboardEvent             : IUIEvent, Modifiers, IsComposing, Keys { }
public interface IMouseEvent                : IUIEvent, Buttons, Coordinates, Modifiers, RelatedTarget { }
public interface IPointerEvent              : IUIEvent, IMouseEvent, Pointer, Angles, WidthHeight, Pressures, Tilts { }
public interface IProgressEvent             : IBaseEvent, Progress { }
public interface ISubmitEvent               : IBaseEvent, Submitter { }
public interface ITransitionEvent           : IBaseEvent, Animation { }
public interface ITouchEvent                : IUIEvent, Modifiers, Touches { }
public interface IWheelEvent                : IUIEvent, IMouseEvent, Deltas { }








public interface IBaseEvent : Target
{
    /// <summary>
    /// A boolean value indicating whether or not the event bubbles up through the DOM.
    /// </summary>
    bool Bubbles { get; }

    /// <summary>
    /// A boolean value indicating whether the event is cancelable.
    /// </summary>
    bool Cancelable { get; }
    
    /// <summary>
    /// A boolean indicating whether or not the event can bubble across the boundary 
    /// between the shadow DOM and the regular DOM.
    /// </summary>
    bool Composed { get; }

    /// <summary>
    /// A reference to the currently registered target for the event. This is the 
    /// object to which the event is currently slated to be sent. It's possible 
    /// this has been changed along the way through retargeting.
    /// </summary>
    EventTarget CurrentTarget { get; }

    /// <summary>
    /// Indicates whether or not the call to event.preventDefault() canceled the 
    /// event.
    /// </summary>
    bool DefaultPrevented { get; }

    /// <summary>
    /// Indicates which phase of the event flow is being processed. It is one of 
    /// the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    /// </summary>
    EventPhase EventPhase { get; }
    
    /// <summary>
    /// Indicates whether or not the event was initiated by the browser (after a 
    /// user click, for instance) or by a script (using an event creation method, 
    /// for example).
    /// </summary>
    bool IsTrusted { get; }
    
    /// <summary>
    /// The time at which the event was created (in milliseconds). By specification, 
    /// this value is time since epoch—but in reality, browsers' definitions vary. 
    /// In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
    /// </summary>
    double TimeStamp { get; }

    /// <summary>
    /// The name identifying the type of the event.
    /// </summary>
    string Type { get; }
}

public interface IInputEvent<T> : IUIEvent, Data, Event.Subsets.DataTransfer, IsComposing
{
    /// <summary>
    /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
    /// </summary>
    string InputType { get; }

    /// <summary>
    /// A reference to the object to which the event was originally dispatched.
    /// </summary>
    new EventTarget<T> Target { get; }
}

public interface IUIEvent: IBaseEvent
{
    /// <summary>
    /// Returns a long with details about the event, depending on the event type.
    /// </summary>
    long Detail { get; }
}