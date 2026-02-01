using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IModifiers : ISubset, ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift, IView
        {
            new const string Format = 
                ModifierAlt.Format + "," + 
                ModifierCtrl.Format + "," + 
                ModifierMeta.Format + "," + 
                ModifierShift.Format;
        }
    }
}