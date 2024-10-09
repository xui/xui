using System.Buffers;

namespace Xui.Web.Composers;

public abstract class BufferWriterComposer(IBufferWriter<byte> writer) : BaseComposer()
{
    public IBufferWriter<byte> Writer { get; set; } = writer;
}