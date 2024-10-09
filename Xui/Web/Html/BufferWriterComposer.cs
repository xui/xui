using System.Buffers;

namespace Xui.Web;

public abstract class BufferWriterComposer(IBufferWriter<byte> writer) : Composer()
{
    public IBufferWriter<byte> Writer { get; set; } = writer;
}