using System.Buffers;
using System.Text;
using Xui.Web.Html;

namespace Xui.Web.Html;

public static class PipeWriterExtensions
{
    public static void Write(this IBufferWriter<byte> writer, int value)
    {
        var destination = writer.GetSpan();
        value.TryFormat(destination, out int length);
        writer.Advance(length);
    }

    public static void Write(this IBufferWriter<byte> writer, string value)
    {
        Write(writer, value.AsSpan());
    }

    public static void Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> value)
    {
        Encoding.UTF8.GetBytes(value, writer);
    }

    public static void Write(this IBufferWriter<byte> writer, ref Chunk chunk)
    {
        chunk.Write(writer);
    }

    public static void WriteStringLiteral(this IBufferWriter<byte> writer, string value)
    {
        // TODO: Does it help to cache the UTF16 -> UTF8 encodings?  
        // Maybe not because of SIMD optimizations?
        Write(writer, value);
    }
}