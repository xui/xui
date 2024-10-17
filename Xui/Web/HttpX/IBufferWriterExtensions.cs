using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX;

public static class IBufferWriterExtensions
{
    public static void Inject(
        this IBufferWriter<byte> writer, 
        [InterpolatedStringHandlerArgument("writer")] ref RawText text)
    {
    }
}