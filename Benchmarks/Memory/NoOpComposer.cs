using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Web4.Composers;

public struct NoOpComposer(IBufferWriter<byte> writer) : IComposer, IStreamingComposer, IDisposable
{
    public IBufferWriter<byte> Writer { get; set; } = writer;

    public bool WriteImmutableMarkup(ref Html parent, string literal) => true;

    public bool WriteMutableValue(ref Html parent, string value) => true;
    public bool WriteMutableValue(ref Html parent, bool value) => true;
    public bool WriteMutableValue(ref Html parent, Color value, string? format = null) => true;
    public bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => true;
    public bool WriteMutableValue<T>(ref Html parent, T value, string? format = null) where T : struct, IUtf8SpanFormattable => true;

    public bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => true;
    public bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => true;
    public bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => true;
    public bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => true;

    public bool WriteMutableNode<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        foreach (var partial in partials) { }
        return true;
    }


    public void OnHtmlPartialBegins(ref Html parent) { }
    public bool OnHtmlPartialEnds(ref Html parent, scoped Html partial, string? format = null, string? expression = null) => true;

    public IComposer Init()
    {
        return this;
    }

    public void Grow(ref Html html, int literalLength, int formattedCount)
    {
    }

    public void Dispose()
    {
        BaseComposer.Current = null;
    }
}