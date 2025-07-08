using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Web4.Composers;
using System.Drawing;

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

    public static int GetMaxPossibleLength(this Color color) => 9;

    public static bool TryFormat(this Color color, Span<byte> utf8Destination, out int bytesWritten)
        => color.TryFormat(utf8Destination, out bytesWritten, "rgb");

    public static bool TryFormat(this Color color, Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format)
    {
        utf8Destination[0] = (byte)'#';
        utf8Destination = utf8Destination[1..];

        var success = format switch
        {
            "ARGB" => color.ToArgb().TryFormat(utf8Destination, out bytesWritten, "X8"),
            "argb" => color.ToArgb().TryFormat(utf8Destination, out bytesWritten, "x8"),
            "RGB" => color.ToRgb().TryFormat(utf8Destination, out bytesWritten, "X6"),
            "rbg" => color.ToRgb().TryFormat(utf8Destination, out bytesWritten, "x6"),
            _ => color.ToRgb().TryFormat(utf8Destination, out bytesWritten, "x6"),
        };

        bytesWritten++;
        return true;
    }

    /// <summary>
    /// If the alpha channel is fully opaque, simplify by using 6 digits instead of 8.
    /// </summary>
    public static int ToRgb(this Color color)
        => (color.A == byte.MaxValue)
            ? (color.ToArgb() & 0xFFFFFF)
            : color.ToArgb();
}