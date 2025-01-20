namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Data
        {
            const string Format = "data";
            string data { get; }
        }

        public interface Buttons
        {
            const string Format = "button,buttons";
            int button { get; }
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
            double x { get; }
        }

        public interface Y
        {
            const string Format = "y";
            double y { get; }
        }

        public interface ClientXY
        {
            const string Format = "clientX,clientY";
            double clientX { get; }
            double clientY { get; }
        }

        public interface MovementXY
        {
            const string Format = "movementX,movementY";
            double movementX { get; }
            double movementY { get; }
        }

        public interface OffsetXY
        {
            const string Format = "offsetX,offsetY";
            double offsetX { get; }
            double offsetY { get; }
        }

        public interface PageXY
        {
            const string Format = "pageX,pageY";
            double pageX { get; }
            double pageY { get; }
        }

        public interface ScreenXY
        {
            const string Format = "screenX,screenY";
            double screenX { get; }
            double screenY { get; }
        }

        public interface Deltas
        {
            const string Format = "deltaX,deltaY,deltaZ,deltaMode";
            double deltaX { get; }
            double deltaY { get; }
            double deltaZ { get; }
            long deltaMode { get; }
        }

        public interface Touches
        {
            const string Format = "changedTouches,targetTouches,touches";
            TouchPoint[] changedTouches { get; }
            TouchPoint[] targetTouches { get; }
            TouchPoint[] touches { get; }
        }

        public interface RelatedTarget
        {
            const string Format = "relatedTarget";
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
            bool altKey { get; }
        }

        public interface ModifierCtrl
        {
            const string Format = "ctrlKey";
            bool ctrlKey { get; }
        }

        public interface ModifierMeta
        {
            const string Format = "metaKey";
            bool metaKey { get; }
        }

        public interface ModifierShift
        {
            const string Format = "shiftKey";
            bool shiftKey { get; }
        }

        public interface IsComposing
        {
            const string Format = "isComposing";
            bool isComposing { get; }
        }
    }
}

#pragma warning disable IDE1006