using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface Modifiers : ModifierAlt, ModifierCtrl, ModifierMeta, ModifierShift
        {
            new const string Format = 
                ModifierAlt.Format + "," + 
                ModifierCtrl.Format + "," + 
                ModifierMeta.Format + "," + 
                ModifierShift.Format;
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.Modifiers> listener, 
        string? format = Event.Subsets.Modifiers.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Func<Event.Subsets.Modifiers, Task> listener, 
        string? format = Event.Subsets.Modifiers.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}