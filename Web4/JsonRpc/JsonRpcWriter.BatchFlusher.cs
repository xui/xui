using System.Buffers;
using System.Threading.Channels;

namespace Web4.JsonRpc;

partial class JsonRpcWriter
{
    private class BatchFlusher : SynchronizationContext
    {
        public ChannelWriter<ReadOnlySequence<byte>>? Flusher { get; set; }

        public override void Post(SendOrPostCallback d, object? state)
        {
            // TODO: Another allocation (transport.Flush()).  Can I nuke it using state?
            base.Post(s =>
            {
                // Set it again because AspNetWebSocket uses ConfigureAwait(false)
                SetSynchronizationContext(this);

                d(s);

                var flusher = Flusher ?? throw new NullReferenceException("🛑 Cannot use BatchFlusher without a Flusher or expect the system to get into a bad state.  Needs investigating.");
                var writer = JsonRpcWriter.Current(flusher);
                writer.Flush();
            }, state);
        }
    }
}