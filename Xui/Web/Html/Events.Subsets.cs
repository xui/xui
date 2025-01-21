namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Data
        {
            const string Format = "data";

            /// <summary>
            /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
            /// </summary>
            string data { get; }
        }

        public interface Buttons
        {
            const string Format = "button,buttons";

            /// <summary>
            /// The button number that was pressed (if applicable) when the mouse event was fired.
            /// </summary>
            int button { get; }

            /// <summary>
            /// The buttons being pressed (if any) when the mouse event was fired.
            /// </summary>
            int buttons { get; }
        }

        public interface Coordinates : XY, ClientXY, MovementXY, OffsetXY, PageXY, ScreenXY
        {
            new const string Format = 
                XY.Format + "," + 
                ClientXY.Format + "," + 
                MovementXY.Format + "," + 
                OffsetXY.Format + "," + 
                PageXY.Format + "," + 
                ScreenXY.Format;
        }
        public interface XY : X, Y
        {
            new const string Format = X.Format + "," + Y.Format;
        }

        public interface X
        {
            const string Format = "x";

            /// <summary>
            /// Alias for MouseEvent.clientX.
            /// </summary>
            double x { get; }
        }

        public interface Y
        {
            const string Format = "y";

            /// <summary>
            /// Alias for MouseEvent.clientY.
            /// </summary>
            double y { get; }
        }

        public interface ClientXY
        {
            const string Format = "clientX,clientY";

            /// <summary>
            /// The X coordinate of the mouse pointer in viewport coordinates.
            /// </summary>
            double clientX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer in viewport coordinates.
            /// </summary>
            double clientY { get; }
        }

        public interface MovementXY
        {
            const string Format = "movementX,movementY";

            /// <summary>
            /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
            /// </summary>
            double movementX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
            /// </summary>
            double movementY { get; }
        }

        public interface OffsetXY
        {
            const string Format = "offsetX,offsetY";
            
            /// <summary>
            /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
            /// </summary>
            double offsetX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
            /// </summary>
            double offsetY { get; }
        }

        public interface PageXY
        {
            const string Format = "pageX,pageY";

            /// <summary>
            /// The X coordinate of the mouse pointer relative to the whole document.
            /// </summary>
            double pageX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer relative to the whole document.
            /// </summary>
            double pageY { get; }
        }

        public interface ScreenXY
        {
            const string Format = "screenX,screenY";

            /// <summary>
            /// The X coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double screenX { get; }

            /// <summary>
            /// The Y coordinate of the mouse pointer in screen coordinates.
            /// </summary>
            double screenY { get; }
        }

        public interface Deltas
        {
            const string Format = "deltaX,deltaY,deltaZ,deltaMode";

            /// <summary>
            /// Returns a double representing the horizontal scroll amount.
            /// </summary>
            double deltaX { get; }

            /// <summary>
            /// Returns a double representing the vertical scroll amount.
            /// </summary>
            double deltaY { get; }

            /// <summary>
            /// Returns a double representing the scroll amount for the z-axis.
            /// </summary>
            double deltaZ { get; }

            /// <summary>
            /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
            /// </summary>
            long deltaMode { get; }
        }

        public interface Touches
        {
            const string Format = "changedTouches,targetTouches,touches";

            /// <summary>
            /// A TouchList of all the Touch objects representing individual points of contact whose states changed between the previous touch event and this one.
            /// </summary>
            TouchPoint[] changedTouches { get; }

            /// <summary>
            /// A TouchList of all the Touch objects that are both currently in contact with the touch surface and were also started on the same element that is the target of the event.
            /// </summary>
            TouchPoint[] targetTouches { get; }

            /// <summary>
            /// A TouchList of all the Touch objects representing all current points of contact with the surface, regardless of target or changed status.
            /// </summary>
            TouchPoint[] touches { get; }
        }

        public interface RelatedTarget
        {
            const string Format = "relatedTarget";

            /// <summary>
            /// The secondary target for the event, if there is one.
            /// </summary>
            HtmlElement relatedTarget { get; }
        }

        public interface Modifiers : ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift
        {
            new const string Format = 
                ModifierAlt.Format + "," + 
                ModifierCtrl.Format + "," + 
                ModifierMeta.Format + "," + 
                ModifierShift.Format;
        }

        public interface ModifierAlt
        {
            const string Format = "altKey";

            /// <summary>
            /// Returns true if the alt key was down when the event was fired.
            /// </summary>
            bool altKey { get; }
        }

        public interface ModifierCtrl
        {
            const string Format = "ctrlKey";

            /// <summary>
            /// Returns true if the control key was down when the event was fired.
            /// </summary>
            bool ctrlKey { get; }
        }

        public interface ModifierMeta
        {
            const string Format = "metaKey";

            /// <summary>
            /// Returns true if the meta key was down when the event was fired.
            /// </summary>
            bool metaKey { get; }
        }

        public interface ModifierShift
        {
            const string Format = "shiftKey";

            /// <summary>
            /// Returns true if the shift key was down when the event was fired.
            /// </summary>
            bool shiftKey { get; }
        }

        public interface IsComposing
        {
            const string Format = "isComposing";

            /// <summary>
            /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
            /// </summary>
            bool isComposing { get; }
        }
    }
}

#pragma warning disable IDE1006