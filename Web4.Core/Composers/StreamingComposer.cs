using System.Buffers;

namespace Web4.Composers;

public abstract class StreamingComposer : BaseComposer
{
    public required IBufferWriter<byte> Writer { get; set; }
}