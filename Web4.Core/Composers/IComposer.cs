using System.Drawing;

namespace Web4.Composers;

public interface IComposer
{
    IComposer Init();
    void Grow(ref Html html, int literalLength, int formattedCount);
    void OnHtmlPartialBegins(ref Html parent);
    bool OnHtmlPartialEnds(ref Html parent, scoped Html partial, string? format = null, string? expression = null);
    bool WriteImmutableMarkup(ref Html parent, string literal);
    bool WriteMutableValue(ref Html parent, string value);
    bool WriteMutableValue(ref Html parent, bool value);
    bool WriteMutableValue(ref Html parent, Color value, string? format = null);
    bool WriteMutableValue(ref Html parent, Uri value, string? format = null);
    bool WriteMutableValue<T>(ref Html parent, T value, string? format = null) where T : struct, IUtf8SpanFormattable;
    bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null);
    bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null);
    bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null);
    bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null);
    bool WriteMutableNode<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null);
}