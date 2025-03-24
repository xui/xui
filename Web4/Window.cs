using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;
using Web4.Composers;
using Web4.Transports;

namespace Web4;

public struct Window: IDisposable
{
    private static readonly ConcurrentDictionary<string, Window> contextLookup = [];
    private static readonly ObjectPool<DefaultEvent> eventPool = ObjectPool.Create<DefaultEvent>();
    const int BUFFER_LENGTH = 1024;

    private readonly ITransport transport;
    private readonly WebSocket webSocket;

    public static bool TryGet(HttpContext http, out Window httpxContext)
    {
        // TODO: Move to header approach?
        var key = http.Connection.Id;
        return contextLookup.TryGetValue(key, out httpxContext);
    }

    public static async Task<Window> Upgrade(HttpContext http)
    {
        var webSocket = await http.WebSockets.AcceptWebSocketAsync();
        var context = new Window(webSocket);

        // TODO: Switch to header.
        var key = http.Connection.Id;
        contextLookup[key] = context;

        return context;
    }

    private Window(WebSocket webSocket)
    {
        this.webSocket = webSocket;
    }

    public async readonly Task ListenForEvents(WindowBuilder window, CancellationToken cancel)
    {
        await foreach (var message in GetNextMessage(cancel))
        {
            var perf = Debug.PerfCheck("ParseMethod");
            var method = ParseMethod(message);
            perf.Dispose();
            if (method is null)
            {
                Console.WriteLine($"🔴 Could not parse the method from the message.");
                continue;
            }

            perf = Debug.PerfCheck("GetKeyhole");
            var keyhole = GetKeyhole(method, window);
            perf.Dispose();

            if (keyhole is EventListener listener)
            {
                perf = Debug.PerfCheck("CaptureSnapshot (before)");
                var before = CaptureSnapshot(window);
                perf.Dispose();

                perf = Debug.PerfCheck("HandleEvent");
                HandleEvent(listener, message);
                perf.Dispose();

                perf = Debug.PerfCheck("CaptureSnapshot (after)");
                var after = CaptureSnapshot(window);
                perf.Dispose();

                // TODO: State invalidations will not live here
                perf = Debug.PerfCheck("DiffAndSendMutations");
                await DiffAndSendMutations(before, after, cancel);
                perf.Dispose();

                await Debug.Log(before, after);

                before.Dispose();
                after.Dispose();
            }
            else
            {
                // TODO: Possible race condition: as a component gets unloaded
                // messages might pass each other across the network.
                Console.WriteLine($"🔴 Event handler not found for key:{method}");
            }
            Console.WriteLine();
        }
    }

    private async readonly IAsyncEnumerable<ReadOnlySequence<byte>> GetNextMessage(
        [EnumeratorCancellation] 
        CancellationToken cancel = default)
    {
        ReadOnlySequence<byte> sequence;
        var owner = MemoryPool<byte>.Shared.Rent(BUFFER_LENGTH);
        var buffer = owner.Memory;
        while (true)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(buffer, cancel);
                var perf = Debug.PerfCheck("GetNextMessage");
                if (result.MessageType == WebSocketMessageType.Close)
                    break; // TODO: Memory leak?

                sequence = new ReadOnlySequence<byte>(buffer[..result.Count]);

                if (!result.EndOfMessage)
                {
                    var segmentStart = new WebSocketSegment(buffer[..result.Count]);
                    var segmentEnd = segmentStart;
                    while (!result.EndOfMessage)
                    {
                        buffer = MemoryPool<byte>.Shared.Rent(BUFFER_LENGTH).Memory;
                        // TODO: ^ Memory owners in two places need to be disposed at the proper time (outide this method... so who's the owner?)!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        result = await webSocket.ReceiveAsync(buffer, cancel);
                        if (result.MessageType == WebSocketMessageType.Close)
                            break; // TODO: Memory leak?

                        segmentEnd = segmentEnd.Append(buffer[..result.Count]);
                        continue;
                    }
                    sequence = new ReadOnlySequence<byte>(segmentStart, 0, segmentEnd, result.Count);
                }
                perf.Dispose();
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine(ex);
                break;
            }

            yield return sequence;
        }
    }

    private static string? ParseMethod(ReadOnlySequence<byte> sequence)
    {
        string? key = null;
        try
        {
            var reader = new Utf8JsonReader(sequence);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("method"))
                {
                    reader.Read();
                    ReadOnlySpan<byte> value = reader.HasValueSequence
                        ? reader.ValueSequence.ToArray()
                        : reader.ValueSpan;
                    key = Keymaker.GetKeyIfCached(value);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return key;
    }

    // TODO: Ack!  You forgot to move composers to structs.
    // static FindKeyholeComposer? composer = null;
    private static EventListener? GetKeyhole(string? key, WindowBuilder window)
    {
        switch (key)
        {
            case null:
                return null;
            case string s1 when s1.StartsWith("win"):
            case string s2 when s2.StartsWith("doc"):
                if (int.TryParse(key.AsSpan()[3..], out var index))
                    return window.Listeners[index];
                else
                    return null;
            default:
                var composer = new FindKeyholeComposer(key);
                // composer ??= new FindKeyholeComposer(key);
                composer.Compose($"{window.Html()}");
                return composer.Listener;
        }
    }

    // TODO: Ack!  You forgot to move composers to structs.
    // static DiffComposer? diffComposer = null;
    private static Snapshot CaptureSnapshot(WindowBuilder window)
    {
        // diffComposer ??= new DiffComposer();
        var diffComposer = new DiffComposer();
        diffComposer.Compose($"{window.Html()}");
        return diffComposer.Snapshot;
    }
    
    private static void HandleEvent(EventListener listener, ReadOnlySequence<byte> message)
    {
        // TODO: Surround with "batch" concept.
        {
            try
            {
                if (listener.Action is not null)
                {
                    listener.Action();
                }
                else if (listener.ActionEvent is not null)
                {
                    var e = eventPool.Get().Init(message);
                    listener.ActionEvent(e);
                    eventPool.Return(e);
                }
                else if (listener.Func is not null)
                {
                    _ = listener.Func();
                }
                else if (listener.FuncEvent is not null)
                {
                    var e = eventPool.Get().Init(message);
                    _ = listener
                        .FuncEvent(e)
                        .ContinueWith(t => eventPool.Return(e));
                        // TODO: Allocation!  Learn how to Return(e) without capturing.
                }
                else
                {
                    Console.WriteLine("🔴 No event listener to invoke.  You need to investigate this.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    private static IEnumerable<(Keyhole, Keyhole)> GetDiffs(Snapshot before, Snapshot after)
    {
        for (int i = 0; i < after.RootLength; i++)
        {
            ref var keyholeBefore = ref before.Buffer[i];
            ref var keyholeAfter = ref after.Buffer[i];

            switch (keyholeBefore.Type)
            {
                case FormatType.Html:
                case FormatType.View:
                case FormatType.Attribute:
                case FormatType.EventListener:
                    continue;
            }

            if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
            {
                yield return (keyholeBefore, keyholeAfter);
            }
        }
    }

    private readonly ValueTask DiffAndSendMutations(Snapshot before, Snapshot after, CancellationToken cancel)
    {
        using var writer = new JsonRpcWriter();
        for (int i = 0; i < after.RootLength; i++)
        {
            ref var keyholeBefore = ref before.Buffer[i];
            ref var keyholeAfter = ref after.Buffer[i];

            switch (keyholeBefore.Type)
            {
                // TODO: Implement
                case FormatType.Html:
                case FormatType.View:
                case FormatType.Attribute:
                case FormatType.EventListener:
                    continue;
            }

            if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
            {
                if (writer.BatchCount == 0)
                    writer.BeginBatch();
                
                writer.WriteRpc("mutate", ref keyholeAfter);
            }
        }

        if (writer.BatchCount > 0)
        {
            writer.EndBatch();
            return webSocket.SendAsync(writer.Memory, WebSocketMessageType.Text, true, cancel);
        }

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        // TODO: Implement cleanup?  Delayed cleanup?
    }

    internal class WebSocketSegment : ReadOnlySequenceSegment<byte>
    {
        public WebSocketSegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        public WebSocketSegment Append(ReadOnlyMemory<byte> memory)
        {
            var segment = new WebSocketSegment(memory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = segment;
            return segment;
        }
    }
}