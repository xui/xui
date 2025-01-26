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
        Action<Events.Subsets.Modifiers> eventHandler, 
        string? format = Events.Subsets.Modifiers.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.Modifiers, Task> eventHandler, 
        string? format = Events.Subsets.Modifiers.Format, 
        [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) 
            => AppendEventHandler(eventHandler, format, expression);
}