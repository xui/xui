using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom
{
    namespace Events.Subsets
    {
        public interface IModifiers : ISubset, ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift, IView
        {
            new const string TRIM = $"{ModifierAlt.TRIM},{ModifierCtrl.TRIM},{ModifierMeta.TRIM},{ModifierShift.TRIM}";
        }
    }
}