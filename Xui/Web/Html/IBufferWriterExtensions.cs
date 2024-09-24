using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xui.Web;

public static class IBufferWriterExtensions
{
    public static void Write(this IBufferWriter<byte> writer, int value)
    {
        var destination = writer.GetSpan();
        value.TryFormat(destination, out int length);
        writer.Advance(length);
    }

    public static void Write(this IBufferWriter<byte> writer, ref Chunk chunk)
    {
        chunk.Write(writer);
    }

    public static void WriteStringLiteral(this IBufferWriter<byte> writer, string value)
    {
        // TODO: Does it help to cache the UTF16 -> UTF8 encodings?  
        // Maybe not because of SIMD optimizations?
        Encoding.UTF8.GetBytes(value, writer);
    }

    public static int Write(
        this IBufferWriter<byte> writer, 
        [InterpolatedStringHandlerArgument("writer")] ref Html html)
    {
        return 4;
    }

    public static int Write(
        this IBufferWriter<byte> writer, 
        Composer composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html)
    {
        return 4;
    }
}