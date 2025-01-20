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
        DataTransfer dataTransfer { get; }
        string inputType { get; }
    }

    public interface Keyboard: UI, Subsets.Modifiers, Subsets.IsComposing
    {
        new const string Format = "code,key,location,repeat," + 
            UI.Format + "," + 
            Subsets.IsComposing.Format + "," +
            Subsets.Modifiers.Format;
        string code { get; }
        string key { get; }
        long location { get; }
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
