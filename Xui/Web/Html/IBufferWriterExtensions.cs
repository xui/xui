using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web;

public static class IBufferWriterExtensions
{
    public static int Write(
        this IBufferWriter<byte> writer, 
        [InterpolatedStringHandlerArgument("writer")] ref Html html)
    {
        return 4;
    }

    public static int Write(
        this IBufferWriter<byte> writer, 
        StreamingComposer composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html)
    {
        return 4;
    }
}