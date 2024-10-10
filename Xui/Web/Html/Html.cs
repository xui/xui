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

    public readonly bool AppendLiteral(string value) => composer.AppendLiteral(value);
    public readonly bool AppendFormatted(string value) => composer.AppendFormatted(value);
    public readonly bool AppendFormatted(int value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(long value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(float value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(double value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(decimal value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(bool value) => composer.AppendFormatted(value);
    public readonly bool AppendFormatted(DateTime value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted(TimeSpan value, string? format = null) => composer.AppendFormatted(value, format);
    public readonly bool AppendFormatted<TView>(TView v) where TView : IView => AppendFormatted(v.Render());
    public readonly bool AppendFormatted(Html html) => composer.AppendFormatted(html);
    public readonly bool AppendFormatted(Slot slot) => AppendFormatted(slot());
    public readonly bool AppendFormatted(Action action) => composer.AppendFormatted(action);
    public readonly bool AppendFormatted(Action<Event> action) => composer.AppendFormatted(action);
    public readonly bool AppendFormatted(Func<Task> actionAsync) => composer.AppendFormatted(actionAsync);
    public readonly bool AppendFormatted(Func<Event, Task> actionAsync) => composer.AppendFormatted(actionAsync);
}