using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web;

public delegate Html Slot();

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public readonly ref struct Html
{
    readonly BaseComposer composer;

    public Html(int literalLength, int formattedCount)
    {
        // For now, do not allow the creation of Html instances detached from the root-node.
        this.composer = BaseComposer.Current ?? throw new ArgumentNullException("Composer.Current");
        composer.Grow(literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        this.composer = BaseComposer.Current ??= new DefaultComposer(writer);
        composer.Grow(literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer, BufferWriterComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Writer = writer;
        composer.Grow(literalLength, formattedCount);
    }

    public readonly void AppendLiteral(string value) => composer.AppendLiteral(value);
    public readonly void AppendFormatted(string value) => composer.AppendFormatted(value);
    public readonly void AppendFormatted(int value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted(long value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted(float value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted(double value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted(decimal value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted(bool value) => composer.AppendFormatted(value);
    public readonly void AppendFormatted(DateTime value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted(TimeSpan value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly void AppendFormatted<TView>(TView v) where TView : IView => AppendFormatted(v.Render());
    public readonly void AppendFormatted(Html html) => composer.AppendFormatted(html);
    public readonly void AppendFormatted(Slot slot) => AppendFormatted(slot());
    public readonly void AppendFormatted(Action action) => composer.AppendFormatted(action);
    public readonly void AppendFormatted(Action<Event> action) => composer.AppendFormatted(action);
    public readonly void AppendFormatted(Func<Task> actionAsync) => composer.AppendFormatted(actionAsync);
    public readonly void AppendFormatted(Func<Event, Task> actionAsync) => composer.AppendFormatted(actionAsync);
}