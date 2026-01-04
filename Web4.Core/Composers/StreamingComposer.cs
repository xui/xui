using System.Buffers;

namespace Web4.Composers;

public abstract class StreamingComposer(IBufferWriter<byte> writer) : BaseComposer
{
    public IBufferWriter<byte> Writer { get; set; } = writer;

    protected T Set<T>(IBufferWriter<byte> writer)
        where T : StreamingComposer
    {
        Writer = writer;
        return (T)this;
    }

    public override void Reset()
    {
        Writer = null!;
        base.Reset();
    }
}