using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Web4.Composers;

namespace Web4;

public static partial class HtmlExtensions
{
    public static void Write(
        this IBufferWriter<byte> writer, // This one defaults to HtmlComposer (see Html constructor below)
        [InterpolatedStringHandlerArgument("writer")] ref Html html)
    {
        html.Dispose();
    }

    public static void Write(
        this IBufferWriter<byte> writer,
        StreamingComposer composer, // This one lets you supply your own composer.
        [InterpolatedStringHandlerArgument("composer")] ref Html html)
    {
        html.Dispose();
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer, // This one defaults to HtmlComposer (see Html constructor below)
        [InterpolatedStringHandlerArgument("writer")] ref Html html,
        CancellationToken cancel = default)
    {
        html.Dispose();
        return writer.FlushAsync(cancel);
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer,
        StreamingComposer composer, // This one lets you supply your own composer.
        [InterpolatedStringHandlerArgument("composer")] ref Html html,
        CancellationToken cancel = default)
    {
        html.Dispose();
        return writer.FlushAsync(cancel);
    }
}

public ref partial struct Html
{
    // Enables the pattern: pipeWriter.Write($"...");
    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
        : this(literalLength, formattedCount, -1, scopedComposer = HtmlComposer.Shared(writer))
    {
    }
}