using System.Buffers;
using System.Drawing;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Web4.Composers;

namespace Web4;

public static partial class BufferWriterExtensions
{
    public static bool WriteUtf8(this IBufferWriter<byte> bufferWriter, string text)
    {
        Span<byte> utf8buffer = bufferWriter.GetSpan(text.Length);
        int length = Encoding.UTF8.GetBytes(text, utf8buffer);
        bufferWriter.Advance(length);
        return true;
    }

    public static bool WriteUtf8(this IBufferWriter<byte> bufferWriter, ReadOnlySpan<byte> bytes)
    {
        Span<byte> utf8buffer = bufferWriter.GetSpan(bytes.Length);
        bytes.CopyTo(utf8buffer);
        bufferWriter.Advance(bytes.Length);
        return true;
    }

    public static bool WriteUtf8<T>(this IBufferWriter<byte> bufferWriter, T value, string? format = null)
        where T : struct, IUtf8SpanFormattable
    {
        // TODO: Research the true max length of T.
        Span<byte> destination = bufferWriter.GetSpan(128);
        value.TryFormat(destination, out int length, format, null);
        bufferWriter.Advance(length);
        return true;
    }

    public static void WriteUtf8(this IBufferWriter<byte> bufferWriter, ReadOnlyMemory<char> memory)
        => WriteUtf8(bufferWriter, memory.Span);
        
    public static void WriteUtf8(this IBufferWriter<byte> bufferWriter, ReadOnlySpan<char> span)
    {
        Span<byte> buffer = bufferWriter.GetSpan(span.Length);
        int length = Encoding.UTF8.GetBytes(span, buffer);
        bufferWriter.Advance(length);
    }
    
    public static bool WriteUtf8(this IBufferWriter<byte> bufferWriter, Color color, string? format = null)
    {
        Span<byte> utf8buffer = bufferWriter.GetSpan(color.GetMaxPossibleLength());
        color.TryFormat(utf8buffer, out int length, format);
        bufferWriter.Advance(length);
        return true;
    }

    public static void WriteUtf8(
        this IBufferWriter<byte> bufferWriter, 
        ReadOnlySpan<byte> text1,
        ReadOnlySpan<byte> text2,
        ReadOnlySpan<byte> text3)
    {
        bufferWriter.WriteUtf8(text1);
        bufferWriter.WriteUtf8(text2);
        bufferWriter.WriteUtf8(text3);
    }

    // TODO: Remove this one after KeyMaker switches to utf8.
    public static void WriteUtf8(
        this IBufferWriter<byte> bufferWriter, 
        ReadOnlySpan<byte> text1,
        string text2,
        ReadOnlySpan<byte> text3)
    {
        bufferWriter.WriteUtf8(text1);
        bufferWriter.WriteUtf8(text2);
        bufferWriter.WriteUtf8(text3);
    }

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
        : this(literalLength, formattedCount, -1, scopedComposer = HtmlComposer.Reuse(writer))
    {
    }
}