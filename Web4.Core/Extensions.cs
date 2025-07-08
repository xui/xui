using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Web4.Composers;

namespace Web4;

public static class Extensions
{
    public static void Write(
        this IBufferWriter<byte> writer,
        [InterpolatedStringHandlerArgument("writer")] ref Html html)
    {
    }

    public static void Write(
        this IBufferWriter<byte> writer,
        StreamingComposer composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html)
    {
    }
}