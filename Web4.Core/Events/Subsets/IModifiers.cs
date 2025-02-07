using System.Runtime.CompilerServices;
using static Web4.Events.Aliases.Subsets;

namespace Web4
{
    namespace Events.Subsets
    {
        public interface IModifiers : ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift
        {
            new const string Format = 
                ModifierAlt.Format + "," + 
                ModifierCtrl.Format + "," + 
                ModifierMeta.Format + "," + 
                ModifierShift.Format;
        }
    }

    public ref partial struct Html
    {
        public bool AppendFormatted(
            Action<Modifiers> listener, 
            string? format = Modifiers.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);

        public bool AppendFormatted(
            Func<Modifiers, Task> listener, 
            string? format = Modifiers.Format, 
            [CallerArgumentExpression(nameof(listener))] string? expression = null) 
                => AppendEventListener(listener, format, expression);
    }
}