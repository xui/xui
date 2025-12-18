#pragma warning disable CS9113 // Parameter is unread.
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Web4;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public readonly ref struct RawText(int literalLength, int formattedCount, IBufferWriter<byte> writer)
{
    public readonly void AppendLiteral(ReadOnlySpan<char> value)
    {
        Encoding.UTF8.GetBytes(value, writer);
    }

    public readonly void AppendFormatted(ReadOnlySpan<char> value)
    {
        Encoding.UTF8.GetBytes(value, writer);
    }

    public readonly void AppendFormatted<T>(T value, string? format = null) where T : struct, IUtf8SpanFormattable
    {
        // TODO: Does Writer.GetSpan() need a length?  What's the max length of all T's?
        const int tMaxLength = 128;

        Span<byte> destination = writer.GetSpan(tMaxLength);
        value.TryFormat(destination, out int length, format, null);
        writer.Advance(length);
    }
}

public static class RawTextExtensions
{
    public static void WriteRaw(
        this IBufferWriter<byte> writer,
        [InterpolatedStringHandlerArgument("writer")] ref RawText text)
    {
    }
}

#pragma warning restore CS9113 // Parameter is unread.