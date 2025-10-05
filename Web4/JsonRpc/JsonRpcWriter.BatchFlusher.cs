using System.Buffers;
using System.Threading.Channels;

namespace Web4.JsonRpc;

partial class JsonRpcWriter
{
    private class BatchFlusher : SynchronizationContext
    {
        private record struct StateHolder(SendOrPostCallback Callback, object? State, BatchFlusher BatchFlusher);
        public ChannelWriter<ReadOnlySequence<byte>>? Flusher { get; set; }

        public override void Post(SendOrPostCallback callback, object? state)
        {
            using var perf = Debug.PerfCheck("BatchFlusher.Post"); // TODO: Remove PerfCheck

            ThreadPool.QueueUserWorkItem<StateHolder>(
                callBack: static s =>
                {
                    // Set it again because AspNetWebSocket uses ConfigureAwait(false)
                    SetSynchronizationContext(s.BatchFlusher);

                    s.Callback(s.State);

                    var flusher = s.BatchFlusher.Flusher ?? throw new NullReferenceException("🛑 Cannot use BatchFlusher without a Flusher or expect the system to get into a bad state.  Needs investigating.");
                    var writer = JsonRpcWriter.Current(flusher);
                    writer.Flush();
                },
                state: new(callback, state, this),
                preferLocal: false
            );
        }
    }
}