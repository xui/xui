namespace Xui.Web;

public partial interface Events
{
    public interface Composition : UI, Subsets.Data
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Data.Format;
    }

    public interface Focus : UI, Subsets.RelatedTarget
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.RelatedTarget.Format;
    }

    public interface Input : UI, Subsets.Data, Subsets.IsComposing
    {
        new const string Format = "dataTransfer,inputType," + 
            UI.Format + "," + 
            Subsets.IsComposing.Format + "," +
            Subsets.Data.Format;
        
        /// <summary>
        /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
        /// </summary>
        DataTransfer DataTransfer { get; }

        /// <summary>
        /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
        /// </summary>
        string InputType { get; }
    }

    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing
    {
        new const string Format = "code,key,location,repeat," + 
            UI.Format + "," + 
            Subsets.IsComposing.Format + "," +
            Subsets.Modifiers.Format;
        
        /// <summary>
        /// Returns a string with the code value of the physical key represented by the event.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Returns a string representing the key value of the key represented by the event.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Returns a number representing the location of the key on the keyboard or other input device.
        /// </summary>
        KeyLocation Location { get; }

        /// <summary>
        /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
        /// </summary>
        bool Repeat { get; }
    }

    public interface Mouse : UI, Subsets.Buttons, Subsets.Coordinates, Subsets.Modifiers, Subsets.RelatedTarget
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Buttons.Format + "," + 
            Subsets.Coordinates.Format + "," + 
            Subsets.Modifiers.Format + "," +
            Subsets.RelatedTarget.Format;
    }

    public interface Touch: UI, Subsets.Modifiers, Subsets.Touches
    {
        new const string Format = 
            UI.Format + "," + 
            Subsets.Modifiers.Format + "," + 
            Subsets.Touches.Format;
    }

    public interface Wheel : UI, Mouse, Subsets.Deltas
    {
        new const string Format = 
            UI.Format + "," + 
            Mouse.Format + "," + 
            Subsets.Deltas.Format;
    }

    public interface UI
    {
        const string Format = "detail,bubbles,cancelable,composed,currentTarget,defaultPrevented,eventPhase,isTrusted,target,timeStamp,type";
        
        /// <summary>
        /// Returns a long with details about the event, depending on the event type.
        /// </summary>
        long Detail { get; }
        
        /// <summary>
        /// A boolean value indicating whether or not the event bubbles up through the DOM.
        /// </summary>
        bool Bubbles { get; }

        /// <summary>
        /// A boolean value indicating whether the event is cancelable.
        /// </summary>
        bool Cancelable { get; }
        
        /// <summary>
        /// A boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.
        /// </summary>
        bool Composed { get; }

        /// <summary>
        /// A reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.
        /// </summary>
        EventTarget CurrentTarget { get; }

        /// <summary>
        /// Indicates whether or not the call to event.preventDefault() canceled the event.
        /// </summary>
        bool DefaultPrevented { get; }

        /// <summary>
        /// Indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
        /// </summary>
        EventPhase EventPhase { get; }
        
        /// <summary>
        /// Indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).
        /// </summary>
        bool IsTrusted { get; }
        
        /// <summary>
        /// A reference to the object to which the event was originally dispatched.
        /// </summary>
        EventTarget Target { get; }

        /// <summary>
        /// The time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
        /// </summary>
        double TimeStamp { get; }

        /// <summary>
        /// The name identifying the type of the event.
        /// </summary>
        string Type { get; }
    }
}

public enum EventPhase : int
{
    /// <summary>
    /// The event is not being processed at this time.
    /// </summary>
    None = 0,
    /// <summary>
    /// The event is being propagated through the target's ancestor objects. 
    /// This process starts with the Window, then Document, then the HTMLHtmlElement, 
    /// and so on through the elements until the target's parent is reached. 
    /// Event listeners registered for capture mode when EventTarget.addEventListener() 
    /// was called are triggered during this phase.
    /// </summary>
    CapturingPhase = 1,
    /// <summary>
    /// The event has arrived at the event's target. Event listeners registered for 
    /// this phase are called at this time. If Event.bubbles is false, processing 
    /// the event is finished after this phase is complete.
    /// </summary>
    AtTarget = 2,
    /// <summary>
    /// The event is propagating back up through the target's ancestors in reverse order, 
    /// starting with the parent, and eventually reaching the containing Window. 
    /// This is known as bubbling, and occurs only if Event.bubbles is true. 
    /// Event listeners registered for this phase are triggered during this process.
    /// </summary>
    BubblingPhase = 3
}

public enum KeyLocation : int
{
    /// <summary>
    /// The key described by the event is not identified as being located in a particular 
    /// area of the keyboard; it is not located on the numeric keypad (unless it's the NumLock key), 
    /// and for keys that are duplicated on the left and right sides of the keyboard, the key is, 
    /// for whatever reason, not to be associated with that location.  
    /// <br/><br/>Examples include alphanumeric keys on the standard PC 101 US keyboard, 
    /// the NumLock key, and the space bar.
    /// </summary>
    Standard = 0,
    /// <summary>
    /// The key is one which may exist in multiple locations on the keyboard and, in this instance, 
    /// is on the left side of the keyboard.
    /// <br/><br/>Examples include the left Control key, the left Command key on a Macintosh 
    /// keyboard, or the left Shift key.
    /// </summary>
    Left = 1,
    /// <summary>
    /// The key is one which may exist in multiple positions on the keyboard and, in this case, 
    /// is located on the right side of the keyboard.
    /// <br/><br/>Examples include the right Shift key and the right Alt key (Option on a Mac keyboard).
    /// </summary>
    Right = 2,
    /// <summary>
    /// The key is located on the numeric keypad, or is a virtual key associated with the numeric 
    /// keypad if there's more than one place the key could originate from. The NumLock key does 
    /// not fall into this group and is always encoded with the location DOM_KEY_LOCATION_STANDARD.
    /// <br/><br/>Examples include the digits on the numeric keypad, the keypad's Enter key, and 
    /// the decimal point on the keypad.
    /// </summary>
    NumPad = 3,
}

public enum DeltaMode : int
{
    /// <summary>
    /// The delta* values are specified in pixels.
    /// </summary>
    Pixel = 0,
    /// <summary>
    /// The delta* values are specified in lines. Each mouse click scrolls a line of content, 
    /// where the method used to calculate line height is browser dependent.
    /// </summary>
    Line = 1,
    /// <summary>
    /// The delta* values are specified in pages. Each mouse click scrolls a page of content.
    /// </summary>
    Page = 2
}

public enum Button : int
{
    /// <summary>
    /// Main button pressed, usually the left button or the un-initialized state
    /// </summary>
    Main = 0,
    /// <summary>
    /// Auxiliary button pressed, usually the wheel button or the middle button (if present)
    /// </summary>
    Auxiliary = 1,
    /// <summary>
    /// Secondary button pressed, usually the right button
    /// </summary>
    Secondary = 2,
    /// <summary>
    /// Fourth button, typically the Browser Back button
    /// </summary>
    Fourth = 3,
    /// <summary>
    /// Fifth button, typically the Browser Forward button
    /// </summary>
    Fifth = 4
}

[Flags]
public enum ButtonFlag : int
{
    /// <summary>
    /// No button or un-initialized
    /// </summary>
    None = 0,
    /// <summary>
    /// Primary button (usually the left button)
    /// </summary>
    Primary = 1,
    /// <summary>
    /// Secondary button (usually the right button)
    /// </summary>
    Secondary = 2,
    /// <summary>
    /// Auxiliary button (usually the mouse wheel button or middle button)
    /// </summary>
    Auxiliary = 4,
    /// <summary>
    /// 4th button (typically the "Browser Back" button)
    /// </summary>
    Fourth = 8,
    /// <summary>
    /// 5th button (typically the "Browser Forward" button)
    /// </summary>
    Fifth = 16
}
