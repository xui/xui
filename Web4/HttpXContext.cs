using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Web4;
using Web4.Composers;

namespace Web4;

public struct HttpXContext: IDisposable
{
    private static readonly ConcurrentDictionary<string, HttpXContext> contextLookup = [];
    const int BUFFER_LENGTH = 1024;
    const int STATE_READ_METHOD = 0;
    const int STATE_READ_HEADERS = 1;
    const int STATE_READ_BODY = 2;
    const int STATE_COMPLETED = 3;

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

    public async Task ListenForEvents(WindowBuilder window, CancellationToken cancellationToken)
    {
        await foreach (var message in GetNextMessage(cancellationToken))
        {
            var perf = Debug.PerfCheck("Parse");
            var (key, e) = Parse(message);
            perf.Dispose();

            perf = Debug.PerfCheck("GetKeyhole");
            var listener = window.GetKeyhole(key);
            perf.Dispose();

            perf = Debug.PerfCheck("HandleEvent");
            if (listener is not null)
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
        byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_LENGTH);
        while (true)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            if (result.CloseStatus != null)
            {
                Console.WriteLine($"WebSocket closed: {result.CloseStatusDescription ?? "No description"}");
                break;
            }

            var sequence = new ReadOnlySequence<byte>(buffer, 0, result.Count);

            if (!result.EndOfMessage)
            {
                var segmentStart = new WebSocketSegment(buffer);
                var segmentEnd = segmentStart;
                while (!result.EndOfMessage)
                {
                    buffer = ArrayPool<byte>.Shared.Rent(BUFFER_LENGTH);
                    // TODO: ^ This guy messes up the Rent/Return!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    segmentEnd = segmentEnd.Append(buffer);
                    continue;
                }
                sequence = new ReadOnlySequence<byte>(segmentStart, 0, segmentEnd, result.Count);
            }

            yield return sequence;
        }
    }

    private async readonly Task HandleEvent(
        Func<Event?, Task> listener, 
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
                await listener(e);
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

        // // TODO: State invalidations will not live here
        // if (isChanged)
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

                        // Other token types elided for brevity
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