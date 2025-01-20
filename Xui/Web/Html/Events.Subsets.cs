namespace Xui.Web;

#pragma warning disable IDE1006 // Naming Styles

public partial interface Events
{
    public partial interface Subsets
    {
        public interface XY
        {
            const string Format = "x,y";
            double x { get; }
            double y { get; }
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

        public interface ModifierKeys : ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift
        {
            new const string Format = ModifierAlt.Format + "," + ModifierCtrl.Format + "," + ModifierMeta.Format + "," + ModifierShift.Format;
        }
    }
}

#pragma warning disable IDE1006