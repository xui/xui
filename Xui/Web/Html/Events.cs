namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

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
        DataTransfer dataTransfer { get; }

        /// <summary>
        /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
        /// </summary>
        string inputType { get; }
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
        string code { get; }

        /// <summary>
        /// Returns a string representing the key value of the key represented by the event.
        /// </summary>
        string key { get; }

        /// <summary>
        /// Returns a number representing the location of the key on the keyboard or other input device. A list of the constants identifying the locations is shown above in Keyboard locations.
        /// </summary>
        long location { get; }

        /// <summary>
        /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
        /// </summary>
        bool repeat { get; }
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
        long detail { get; }
        
        /// <summary>
        /// A boolean value indicating whether or not the event bubbles up through the DOM.
        /// </summary>
        bool bubbles { get; }

        /// <summary>
        /// A boolean value indicating whether the event is cancelable.
        /// </summary>
        bool cancelable { get; }
        
        /// <summary>
        /// A boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.
        /// </summary>
        bool composed { get; }

        /// <summary>
        /// A reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.
        /// </summary>
        HtmlElement currentTarget { get; }

        /// <summary>
        /// Indicates whether or not the call to event.preventDefault() canceled the event.
        /// </summary>
        bool defaultPrevented { get; }

        /// <summary>
        /// Indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
        /// </summary>
        int eventPhase { get; } // TODO: Enum?
        
        /// <summary>
        /// Indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).
        /// </summary>
        bool isTrusted { get; }
        
        /// <summary>
        /// A reference to the object to which the event was originally dispatched.
        /// </summary>
        HtmlElement target { get; }

        /// <summary>
        /// The time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
        /// </summary>
        double timeStamp { get; }

        /// <summary>
        /// The name identifying the type of the event.
        /// </summary>
        string type { get; }
    }
}

#pragma warning restore IDE1006 // Naming Styles
