using System.Buffers;

namespace Web4.Composers;

public interface IStreamingComposer
{
    public IBufferWriter<byte> Writer { get; set; }
}