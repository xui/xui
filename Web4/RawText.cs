using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Web4.Composers;

namespace Web4;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public readonly ref struct RawText
{
    private readonly IBufferWriter<byte> writer;

    public RawText(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        this.writer = writer;
    }

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
        Span<byte> destination = writer.GetSpan();
        value.TryFormat(destination, out int length, format, null);
        writer.Advance(length);
    }
}