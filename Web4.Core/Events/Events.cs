using Web4.Events.Subsets;
using static Web4.Events.Aliases.Subsets;

namespace Web4.Events;

public interface IDragEvent                 : IMouseEvent, DataTransfer { }
public interface IPointerEvent              : IMouseEvent, Pointer, Angles, WidthHeight, Pressures, Tilts { }
public interface IWheelEvent                : IMouseEvent, Deltas { }

public interface IMouseEvent                : IUIEvent, Buttons, Coordinates, Modifiers, RelatedTarget { }
public interface ICompositionEvent          : IUIEvent, Data { }
public interface IFocusEvent                : IUIEvent, RelatedTarget { }
public interface IKeyboardEvent             : IUIEvent, Modifiers, IsComposing, Keys { }
public interface ITouchEvent                : IUIEvent, Modifiers, Touches { }

public interface IUIEvent                   : IEvent, Detail { }
public interface IAnimationEvent            : IEvent, Animation { }
public interface IDeviceMotionEvent         : IEvent, DeviceMotion { }
public interface IDeviceOrientationEvent    : IEvent, DeviceOrientation { }
public interface IErrorEvent                : IEvent, Error { }
public interface IHashChangeEvent           : IEvent, HashChange { }
public interface IProgressEvent             : IEvent, Progress { }
public interface ISubmitEvent               : IEvent, Submitter { }
public interface ITransitionEvent           : IEvent, Animation { }

// TODO: Implement
public interface IBeforeUnloadEvent         : IEvent { }
public interface IClipboardEvent            : IEvent { }
public interface IContentVisibilityAutoStateChangeEvent : IEvent { }
public interface IFormDataEvent             : IEvent { }
public interface IGamepadEvent              : IEvent { }
public interface IMessageEvent              : IEvent { }
public interface IPageTransitionEvent       : IEvent { }
public interface IPopStateEvent             : IEvent { }
public interface IPromiseRejectionEvent     : IEvent { }
public interface IStorageEvent              : IEvent { }
public interface IToggleEvent               : IEvent { }

public interface IEvent : Target
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

public interface IInputEvent<T> : IUIEvent, Data, DataTransfer, IsComposing
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