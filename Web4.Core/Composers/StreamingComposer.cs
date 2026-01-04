using System.Buffers;

namespace Web4.Composers;

public abstract class StreamingComposer(IBufferWriter<byte> writer) : BaseComposer
{
    public IBufferWriter<byte> Writer { get; set; } = writer;

    public override void Reset()
    {
        Writer = null!;
    }
}