using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public readonly ref struct Html
{
    readonly BaseComposer composer;

    public Html(int literalLength, int formattedCount)
    {
        // For now, do not allow the creation of Html instances detached from the root-node.
        this.composer = BaseComposer.Current ?? throw new ArgumentNullException("BaseComposer.Current");
        composer.Grow(literalLength, formattedCount);
        composer.PrependHtml();
    }

    public Html(int literalLength, int formattedCount, BaseComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Grow(literalLength, formattedCount);
        composer.PrependHtml();
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        this.composer = BaseComposer.Current ??= new DefaultComposer(writer);
        composer.Grow(literalLength, formattedCount);
        composer.PrependHtml();
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer, StreamingComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Writer = writer;
        composer.Grow(literalLength, formattedCount);
        composer.PrependHtml();
    }

    public readonly bool AppendLiteral(string markup) => composer.AppendLiteral(markup);
    public readonly bool AppendFormatted(string value) => composer.AppendFormatted(value);
    public readonly bool AppendFormatted(int value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(long value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(float value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(double value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(decimal value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(bool value) => composer.AppendFormatted(value);
    public readonly bool AppendFormatted(DateTime value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(TimeSpan value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(Func<Event, Html> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) => composer.AppendFormatted(attribute, expression);
    public readonly bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, [CallerArgumentExpression(nameof(attribute))] string? expression = null) where T : struct, IUtf8SpanFormattable => composer.AppendFormatted(attribute, format, expression);
    public readonly bool AppendFormatted(Func<Event, bool> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) => composer.AppendFormatted(attribute, expression);
    public readonly bool AppendFormatted(Action eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) => composer.AppendFormatted(eventHandler, expression);
    public readonly bool AppendFormatted(Action<Event> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) => composer.AppendFormatted(eventHandler, expression);
    public readonly bool AppendFormatted(Func<Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) => composer.AppendFormatted(eventHandler, expression);
    public readonly bool AppendFormatted(Func<Event, Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null) => composer.AppendFormatted(eventHandler, expression);
    public readonly bool AppendFormatted<TView>(TView partial) where TView : IView => AppendFormatted(partial.Render());
    public readonly bool AppendFormatted(Slot slot) => AppendFormatted(slot());
    public readonly bool AppendFormatted(Html partial) => composer.AppendFormatted(partial);
}