using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Web4;

public struct HttpXContext: IDisposable
{
    private static readonly ConcurrentDictionary<string, HttpXContext> contextLookup = [];
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
            var perf = Debug.PerfCheck("Parse");
            var (key, e) = Parse(message);
            perf.Dispose();

            perf = Debug.PerfCheck("GetKeyhole");
            var keyhole = window.GetKeyhole(key);
            perf.Dispose();

            perf = Debug.PerfCheck("HandleEvent");
            if (keyhole is EventListener listener)
            {
                await HandleEvent(listener, e, window, cancellationToken);
            }
            else
            {
                // TODO: Possible race condition: as a component gets unloaded
                // messages might pass each other across the network.
                Console.WriteLine($"Event handler not found for key:{key}");
            }
            perf.Dispose();
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
            var perf = Debug.PerfCheck("GetNextMessage");
            try
            {
                var result = await webSocket.ReceiveAsync(buffer, cancellationToken);
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
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine(ex);
                break;
            }

            perf.Dispose();
            yield return sequence;
        }
    }

    private async readonly Task HandleEvent(
        EventListener listener,
        Event? e, 
        WindowBuilder window, 
        CancellationToken cancellationToken = default)
    {
        // UIThread.Run(async () => 
        // {
        //     await listener(domEvent!);
        // });

        // State.Invoke(async () => 
        // {
        //     await listener(domEvent!);
        // });

        var isChanged = false;
        // TODO: Surround with "batch" concept.
        {
            var before = window.CaptureSnapshot();
            try
            {
                await listener.Invoke(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            var after = window.CaptureSnapshot();

            for (int i = 0; i < after.RootLength; i++)
            {
                ref var keyholeBefore = ref before.Buffer[i];
                ref var keyholeAfter = ref after.Buffer[i];

                if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
                {
                    switch (keyholeAfter.Type)
                    {
                        case FormatType.String:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.String}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Boolean:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.Boolean}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Color:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.Color}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Uri:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.Uri}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Integer:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue={keyholeAfter.Integer}"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Long:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue={keyholeAfter.Long}"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Float:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue={keyholeAfter.Float}"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Double:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue={keyholeAfter.Double}"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.Decimal:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue={keyholeAfter.Decimal}"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.DateTime:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.DateTime}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.DateOnly:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.DateOnly}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.TimeSpan:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.TimeSpan}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                        case FormatType.TimeOnly:
                            isChanged = true;
                            await webSocket.SendAsync(Encoding.UTF8.GetBytes($"ui.{keyholeAfter.Key}.nodeValue=`{keyholeAfter.TimeOnly}`"), WebSocketMessageType.Text, true, cancellationToken);
                            break;
                    }
                }
            }
        }

        // TODO: State invalidations will not live here
        if (isChanged)
            Console.WriteLine($"isChanged:{isChanged}");
        //     await window.DebugSnapshot(Pipe.Output);
    }
    
    private static (string?, Event?) Parse(ReadOnlySequence<byte> sequence)
    {
        string? key = null;
        try
        {
            var reader = new Utf8JsonReader(sequence, new()
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow
            });

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals("method"))
                {
                    reader.Read();
                    key = reader.GetString(); // TODO: Use Keymaker?
                }
Console.Write(reader.TokenType);

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                    case JsonTokenType.String:
                        {
                            string? text = reader.GetString();
Console.Write(" ");
Console.Write(text);
                            break;
                        }

                    case JsonTokenType.Number:
                        {
                            int intValue = reader.GetInt32();
Console.Write(" ");
Console.Write(intValue);
                            break;
                        }
                }
Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

Console.WriteLine($"key:{key}");

        return (key, new HttpXEvent());
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