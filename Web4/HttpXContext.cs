using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.ObjectPool;

namespace Web4;

public struct HttpXContext: IDisposable
{
    private static readonly ConcurrentDictionary<string, HttpXContext> contextLookup = [];
    private static readonly ObjectPool<DefaultEvent> eventPool = ObjectPool.Create<DefaultEvent>();
    const int BUFFER_LENGTH = 1024;

    private readonly WebSocket webSocket;
    public readonly bool IsWebSocketOpen => webSocket.State == WebSocketState.Open;

    public static bool TryGet(HttpContext httpContext, out HttpXContext httpxContext)
    {
        // TODO: Move to header approach?
        var key = httpContext.Connection.Id;
        return contextLookup.TryGetValue(key, out httpxContext);
    }

    public static async Task<HttpXContext> Upgrade(HttpContext httpContext)
    {
        var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
        var context = new HttpXContext(webSocket);

        // TODO: Switch to header.
        var key = httpContext.Connection.Id;
        contextLookup[key] = context;

        return context;
    }

    private HttpXContext(WebSocket webSocket)
    {
        this.webSocket = webSocket;
    }

    public async readonly Task UpdatePath(PathString path)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            // var writer = Pipe.Output;
            // writer.Inject($"window.history.pushState({{}},'', '{ path.ToUriComponent() }')");
            // await writer.FlushAsync();
            await Task.Delay(0);
        }
    }

    public async readonly Task ListenForEvents(WindowBuilder window, CancellationToken cancellationToken)
    {
        await foreach (var message in GetNextMessage(cancellationToken))
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
            var keyhole = window.GetKeyhole(method);
            perf.Dispose();

            if (keyhole is EventListener listener)
            {
                perf = Debug.PerfCheck("CaptureSnapshot (before)");
                var before = window.CaptureSnapshot();
                perf.Dispose();

                perf = Debug.PerfCheck("HandleEvent");
                HandleEvent(listener, message);
                perf.Dispose();

                perf = Debug.PerfCheck("CaptureSnapshot (after)");
                var after = window.CaptureSnapshot();
                perf.Dispose();

                // TODO: State invalidations will not live here
                perf = Debug.PerfCheck("DiffAndSendMutations");
                await DiffAndSendMutations(before, after, cancellationToken);
                perf.Dispose();

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
        CancellationToken cancellationToken = default)
    {
        ReadOnlySequence<byte> sequence;
        var owner = MemoryPool<byte>.Shared.Rent(BUFFER_LENGTH);
        var buffer = owner.Memory;
        while (true)
        {
            try
            {
                var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
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

                        result = await webSocket.ReceiveAsync(buffer, cancellationToken);
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

    private readonly async ValueTask DiffAndSendMutations(Snapshot before, Snapshot after, CancellationToken cancellationToken)
    {
        for (int i = 0; i < after.RootLength; i++)
        {
            ref var keyholeBefore = ref before.Buffer[i];
            ref var keyholeAfter = ref after.Buffer[i];

            if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
            {
                var value = keyholeAfter.Type switch
                {
                    FormatType.String => $"'{keyholeAfter.String}'",
                    FormatType.Boolean => keyholeAfter.Boolean!.Value ? "true" : "false",
                    FormatType.Integer => $"{keyholeAfter.Integer}",
                    FormatType.Long => $"{keyholeAfter.Long}",
                    FormatType.Float => $"{keyholeAfter.Float}",
                    FormatType.Double => $"{keyholeAfter.Double}",
                    FormatType.DateTime => $"'{keyholeAfter.DateTime}'",
                    FormatType.DateOnly => $"'{keyholeAfter.DateOnly}'",
                    FormatType.TimeSpan => $"'{keyholeAfter.TimeSpan}'",
                    FormatType.TimeOnly => $"'{keyholeAfter.TimeOnly}'",
                    FormatType.Color => $"'{keyholeAfter.Color}'",
                    FormatType.Uri => $"'{keyholeAfter.Uri}'",
                    _ => null
                };

                if (value is not null)
                {
                    var message = $$"""
                        {"jsonrpc":"2.0","method":"mutate","params":["{{keyholeAfter.Key}}",{{value}}]}
                        """;
                    await webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }
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