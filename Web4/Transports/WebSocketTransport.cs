using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Web4.Transports;

public class WebSocketTransport : IWeb4Transport, IDisposable
{
    private const int BUFFER_LENGTH = 1024;
    private static readonly ConcurrentDictionary<string, Window> windows = [];

    private readonly HttpContext http;
    private readonly WebSocket webSocket;

    private WebSocketTransport(HttpContext http, WebSocket webSocket)
    {
        this.http = http;
        this.webSocket = webSocket;
    }

    public static async Task<WebSocketTransport> Create(HttpContext http)
    {
        var webSocket = await http.WebSockets.AcceptWebSocketAsync();
        return new WebSocketTransport(http, webSocket);
    }

    public void Dispose()
    {
        webSocket.Dispose();
    }

    public Window GetOrCreateWindow(WindowBuilder builder)
    {
        // TODO: Move to header approach?
        var key = http.Connection.Id;
        if (!windows.TryGetValue(key, out var window))
        {
            window = new Window(this, builder, http.RequestAborted);
            windows[key] = window;
        }
        return window;
    }

    public ValueTask BeginMutations()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Mutate(IEnumerable<int> indexes, Keyhole[] before, Keyhole[] after)
    {
        using var perf = Debug.PerfCheck("Mutate"); // TODO: Remove PerfCheck

        using var writer = new JsonRpcWriter();
        foreach (var index in indexes)
        {
            //ref var keyholeBefore = ref before[index];
            ref var keyholeAfter = ref after[index];

            if (writer.BatchCount == 0)
                writer.BeginBatch();

            writer.WriteRpc("mutate", ref keyholeAfter);
        }

        if (writer.BatchCount > 0)
        {
            writer.EndBatch();
            return webSocket.SendAsync(writer.Memory, WebSocketMessageType.Text, true, http.RequestAborted);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask EndMutations(CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }

    public async Task ListenForRpcMessages(Window window)
    {
        await foreach (var message in GetNextMessage())
        {
            var rpcMethod = ParseMethod(message);
            if (rpcMethod is null)
            {
                Console.WriteLine($"🔴 Could not parse the method from the message.");
                continue;
            }

            var rpcEvent = new WebSocketEvent(message);
            window.Invalidate(); // TODO: This line doesn't belong here.
            window.HandleEvent(rpcMethod, ref rpcEvent);
            window.RequestUpdate(); // TODO: This line doesn't belong here.
        }
    }

    private async IAsyncEnumerable<ReadOnlySequence<byte>> GetNextMessage()
    {
        ReadOnlySequence<byte> sequence;
        while (true)
        {
            try
            {
                var buffer = ArrayPool<byte>.Shared.Rent(BUFFER_LENGTH);
                var result = await webSocket.ReceiveAsync(buffer, http.RequestAborted);

                Console.WriteLine();
                using var perf = Debug.PerfCheck("GetNextMessage"); // TODO: Remove PerfCheck

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    break;
                }
                else if (result.EndOfMessage)
                {
                    sequence = new ReadOnlySequence<byte>(buffer, 0, result.Count);
                }
                else
                {
                    // TODO: Debug through this to verify that it works the way you think it does.
                    // Pay particularly close attention to Segment.Return – does it return buffers
                    // to the pool like you think it does?
                    var segmentStart = new Segment(buffer, 0..result.Count);
                    var segmentEnd = segmentStart;
                    while (!result.EndOfMessage)
                    {
                        buffer = ArrayPool<byte>.Shared.Rent(BUFFER_LENGTH);
                        result = await webSocket.ReceiveAsync(buffer, http.RequestAborted);
                        segmentEnd = segmentEnd.Append(buffer, 0..result.Count);
                    }
                    sequence = new ReadOnlySequence<byte>(segmentStart, 0, segmentEnd, result.Count);
                }
            }
            catch (WebSocketException)
            {
                break;
            }

            yield return sequence;
        }
    }

    private static string? ParseMethod(ReadOnlySequence<byte> sequence)
    {
        using var perf = Debug.PerfCheck("ParseMethod"); // TODO: Remove PerfCheck

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
                        ? reader.ValueSequence.ToArray() // TODO: Allocates, but is rare?  Confirm.
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

    internal class Segment : ReadOnlySequenceSegment<byte>
    {
        private readonly byte[] buffer;

        public Segment(byte[] buffer, Range range)
        {
            this.buffer = buffer;
            Memory = buffer.AsMemory()[range];
        }

        public Segment Append(byte[] buffer, Range range)
        {
            var segment = new Segment(buffer, range)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = segment;
            return segment;
        }

        public static void Return(ReadOnlySequence<byte> sequence)
        {
            if (sequence.IsSingleSegment)
            {
                if (sequence.Start.GetObject() is byte[] buffer)
                    ArrayPool<byte>.Shared.Return(buffer);
            }
            else
            {
                var position = sequence.Start;
                do
                {
                    if (position.GetObject() is Segment segment)
                        ArrayPool<byte>.Shared.Return(segment.buffer);
                } while (sequence.TryGet(ref position, out var memory, advance: true));
            }
        }
    }
}