namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public interface Events
{
    public interface Composition
    {
        const string Format = "data";
        string data { get; }
    }

    public interface Focus
    {
        const string Format = "relatedTarget";
        HtmlElement relatedTarget { get; }
    }

    public interface Input
    {
        const string Format = "data,dataTransfer,inputType,isComposing";
        string data { get; }
        DataTransfer dataTransfer { get; }
        string inputType { get; }
        bool isComposing { get; }
    }

    public interface Keyboard
    {
        const string Format = "altKey,code,ctrlKey,isComposing,key,location,metaKey,repeat,shiftKey";
        bool altKey { get; }
        string code { get; }
        bool ctrlKey { get; }
        bool isComposing { get; }
        string key { get; }
        long location { get; }
        bool metaKey { get; }
        bool repeat { get; }
        bool shiftKey { get; }
    }

    public interface Mouse : UI, MouseXY
    {
        new const string Format = "altKey,button,buttons,clientX,clientY,ctrlKey,metaKey,movementX,movementY,offsetX,offsetY,pageX,pageY,relatedTarget,screenX,screenY,shiftKey" + MouseXY.Format;
        
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

        public interface XY : MouseXY { }
    }

    public interface MouseXY
    {
        const string Format = "x,y";

        double x { get; }
        double y { get; }
    }

    public interface Touch
    {
        const string Format = "altKey,altKey,changedTouches,ctrlKey,metaKey,shiftKey,targetTouches,touches";
        bool altKey { get; }
        TouchPoint[] changedTouches { get; }
        bool ctrlKey { get; }
        bool metaKey { get; }
        bool shiftKey { get; }
        TouchPoint[] targetTouches { get; }
        TouchPoint[] touches { get; }

    }

    public interface Wheel : Mouse
    {
        new const string Format = "deltaX,deltaY,deltaZ,deltaMode";
        double deltaX { get; }
        double deltaY { get; }
        double deltaZ { get; }
        long deltaMode { get; }
    }

    public interface UI
    {
        const string Format = "detail,bubbles,cancelable,composed,currentTarget,defaultPrevented,eventPhase,isTrusted,target,timeStamp,type";
        long detail { get; }
        bool bubbles { get; }
        bool cancelable { get; }
        bool composed { get; }
        HtmlElement currentTarget { get; }
        bool defaultPrevented { get; }
        int eventPhase { get; }
        bool isTrusted { get; }
        HtmlElement target { get; }
        double timeStamp { get; }
        string type { get; }
    }
}

#pragma warning restore IDE1006 // Naming Styles
