namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial interface Events
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

    public interface Keyboard: Subsets.ModifierKeys, Subsets.ModifierAlt, Subsets.ModifierCtrl, Subsets.ModifierMeta, Subsets.ModifierShift
    {
        new const string Format = "code,isComposing,location,repeat," + Subsets.ModifierKeys.Format;
        string code { get; }
        bool isComposing { get; }
        string key { get; }
        long location { get; }
        bool repeat { get; }
    }

    public interface Mouse : UI, Subsets.XY, Subsets.ModifierKeys, Subsets.ModifierAlt, Subsets.ModifierCtrl, Subsets.ModifierMeta, Subsets.ModifierShift
    {
        new const string Format = "button,buttons,clientX,clientY,movementX,movementY,offsetX,offsetY,pageX,pageY,relatedTarget,screenX,screenY," + Subsets.XY.Format + "," + Subsets.ModifierKeys.Format;
        
        int button { get; }
        int buttons { get; }
        double clientX { get; }
        double clientY { get; }
        double movementX { get; }
        double movementY { get; }
        double offsetX { get; }
        double offsetY { get; }
        double pageX { get; }
        double pageY { get; }
        HtmlElement relatedTarget { get; }
        double screenX { get; }
        double screenY { get; }
    }

    public interface Touch: Subsets.ModifierKeys, Subsets.ModifierAlt, Subsets.ModifierCtrl, Subsets.ModifierMeta, Subsets.ModifierShift
    {
        const string Format = "changedTouches,targetTouches,touches," + Subsets.ModifierKeys.Format;
        TouchPoint[] changedTouches { get; }
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
