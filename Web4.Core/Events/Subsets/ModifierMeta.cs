using System.Runtime.CompilerServices;

namespace Web4;

public partial interface Events
{
    public partial interface Subsets
    {
        public interface ModifierMeta
        {
            const string Format = "metaKey";

            /// <summary>
            /// Returns true if the meta key was down when the event was fired.
            /// </summary>
            bool MetaKey { get; }
        }
    }
}

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Events.Subsets.ModifierMeta> listener, 
        string? format = Events.Subsets.ModifierMeta.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);

    public bool AppendFormatted(
        Func<Events.Subsets.ModifierMeta, Task> listener, 
        string? format = Events.Subsets.ModifierMeta.Format, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventHandler(listener, format, expression);
}