using System.Buffers;

namespace MicroHtml.Composers;

public interface IStreamingComposer
{
    public IBufferWriter<byte> Writer { get; set; }
}