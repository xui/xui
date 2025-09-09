using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Web4.WebSockets;

public class WebSocketTransport : IWeb4Transport, IDisposable
{
    private const int RECEIVE_BUFFER_LENGTH = 1024;
    private static readonly ConcurrentDictionary<string, Window> windows = [];

    private readonly HttpContext http;
    private readonly WebSocket webSocket;

    static WebSocketTransport()
    {
        Keymaker.CacheKey("app.dispatchEvent");
        Keymaker.CacheKey("app.keyholes.dump");
        Keymaker.CacheKey("app.benchmark");
        Keymaker.CacheKey("app.ping");
    }

    private WebSocketTransport(HttpContext http, WebSocket webSocket)
    {
        this.http = http;
        this.webSocket = webSocket;
    }

    public static async Task<WebSocketTransport> Create(HttpContext http)
    {
        // TODO: Move to config
        var context = new WebSocketAcceptContext
        {
            KeepAliveInterval = TimeSpan.FromSeconds(60),
            KeepAliveTimeout = TimeSpan.FromSeconds(20)
        };
        var webSocket = await http.WebSockets.AcceptWebSocketAsync(context);
        return new WebSocketTransport(http, webSocket);
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

    public async Task Disconnect()
    {
        await webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Application ended...",
            CancellationToken.None
        );
    }

    public static void DisconnectAll()
    {
        Parallel.ForEach(
            windows.Values,
            async (window, cancel) => await window.Disconnect()
        );
    }

    public async ValueTask ApplyMutations(Keyhole[] oldBuffer, Keyhole[] newBuffer)
    {
        try
        {
        using var mutationBatch = DiffUtil.CreateBatch<WebSocketMutationBatch>(oldBuffer, newBuffer);
        using var perf = Debug.PerfCheck("webSocket.SendAsync"); // TODO: Remove PerfCheck

        if (mutationBatch.Buffer is ReadOnlyMemory<byte> buffer)
        {
            await webSocket.SendAsync(
                buffer: buffer,
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: http.RequestAborted);
        }
        
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async ValueTask SendResult(int id)
    {
        var writer = JsonRpcWriter.Pool.Get();
        writer.WriteResult(id);

        if (writer.Result is ReadOnlyMemory<byte> buffer)
        {
            await webSocket.SendAsync(
                buffer: buffer,
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: http.RequestAborted);
        }

        JsonRpcWriter.Pool.Return(writer);
    }

    public async Task ListenForRpcMessages(Window app)
    {
        await foreach (var message in GetNextMessage())
        {
            // TODO: This doesn't belong here.
            foreach (var w in windows.Values)
                w.Invalidate();

            try
            {
                var rpc = JsonRpcReader.ParseMessage(message);

                // No awaiting.  This event loop shouldn't be blocked by RPCs.
                switch (rpc)
                {
                    case JsonRpc { Method: "app.dispatchEvent" }:
                        var dispatchEventParams = JsonRpcReader.ParseParams(message);
                        var key = dispatchEventParams.GetNextString();
                        var @event = dispatchEventParams.GetNextEvent();
                        var propagationID = dispatchEventParams.GetNextInt();
                        app.DispatchEvent(key, @event, propagationID);
                        break;

                    case JsonRpc { Method: "app.keyholes.dump" }:
                        app.DumpKeyholes(webSocket);
                        break;

                    case JsonRpc { Method: "app.benchmark" }:
                        var benchmarkParams = JsonRpcReader.ParseParams(message);
                        var threads = benchmarkParams.GetNextInt(); // TODO: This is supposed to be an int? not an int.
                        app.Benchmark(threads);
                        break;

                    case JsonRpc { Method: "app.ping", ID: int requestID }:
                        app.Ping();
                        await SendResult(requestID);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JsonRpc message:\n{ex}");
            }

            // TODO: This doesn't belong here.
            foreach (var w in windows.Values)
                w.RequestUpdate();
        }
    }

    private async IAsyncEnumerable<ReadOnlySequence<byte>> GetNextMessage()
    {
        ReadOnlySequence<byte> sequence;
        while (true)
        {
            try
            {
                var buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
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
                        buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
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
            // TODO: I've never verified with my own eyes that this works as expected.
            // So create both types here, verify they don't allocate, don't leak, and
            // verify they are of the type you are expecting such that they Return().

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
    
    public void Dispose()
    {
        webSocket.Dispose();
    }
}