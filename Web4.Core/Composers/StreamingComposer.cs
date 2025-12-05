using System.Buffers;

namespace Web4.Composers;

public interface IStreamingComposer : IComposer
{
    IBufferWriter<byte> Writer { get; set; }
}

// public abstract class StreamingComposer(IBufferWriter<byte> writer) : BaseComposer(), IStreamingComposer
// {
//     public IBufferWriter<byte> Writer { get; set; } = writer;
// }