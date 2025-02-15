using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Web4;
using Web4.Composers;

namespace Web4;

public struct HttpXContext(WebSocketPipe? pipe)
{
    const int STATE_READ_METHOD = 0;
    const int STATE_READ_HEADERS = 1;
    const int STATE_READ_BODY = 2;
    const int STATE_COMPLETED = 3;

    public WebSocketPipe? Pipe { get; set; } = pipe;
    public readonly bool IsWebSocketOpen => Pipe is not null && Pipe.State == WebSocketState.Open;

    public static HttpXContext Get(HttpContext httpContext)
    {
        // TODO: Move to header approach?
        var key = httpContext.Connection.Id;
        var pipe = WebSocketPipe.Get(key);
        return new HttpXContext(pipe);
    }

    public async readonly Task UpdatePath(PathString path)
    {
        if (Pipe is not null && Pipe.State == WebSocketState.Open)
        {
            var writer = Pipe.Output;
            writer.Inject($"window.history.pushState({{}},'', '{ path.ToUriComponent() }')");
            await writer.FlushAsync();
        }
    }

    public async Task ListenForEvents(WindowBuilder window, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(Pipe);

        await Task.WhenAll(
            Receive(window), 
            Pipe.RunAsync(cancellationToken)
        );
    }

    private async Task Receive(WindowBuilder window)
    {
        if (Pipe is null)
            return;

        while (await Pipe.Input.ReadAsync() is var result && !result.IsCompleted)
        {
            // TODO: Buffer might be multiple segments one day.
            var buffer = result.Buffer.First;

            // long gc1 = GC.GetAllocatedBytesForCurrentThread();
            // var sw1 = Stopwatch.GetTimestamp();
            var (key, domEvent) = ParseEvent(buffer.Span);
            // var elapsed = Stopwatch.GetElapsedTime(sw1);
            // long gc2 = GC.GetAllocatedBytesForCurrentThread();
            // Console.WriteLine($"ParseEvent: key:{key} elapsed: {elapsed.TotalNanoseconds} ns, allocations: {(gc2 - gc1):n0} bytes");

            Pipe.Input.AdvanceTo(result.Buffer.End);

            if (window.GetKeyhole(key) is Func<Event?, Task> listener)
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
                        await listener(domEvent);
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
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.String}`"));
                                    break;
                                case FormatType.Boolean:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.Boolean}`"));
                                    break;
                                case FormatType.Color:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.Color}`"));
                                    break;
                                case FormatType.Uri:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.Uri}`"));
                                    break;
                                case FormatType.Integer:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue={keyholeAfter.Integer}"));
                                    break;
                                case FormatType.Long:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue={keyholeAfter.Long}"));
                                    break;
                                case FormatType.Float:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue={keyholeAfter.Float}"));
                                    break;
                                case FormatType.Double:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue={keyholeAfter.Double}"));
                                    break;
                                case FormatType.Decimal:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue={keyholeAfter.Decimal}"));
                                    break;
                                case FormatType.DateTime:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.DateTime}`"));
                                    break;
                                case FormatType.DateOnly:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.DateOnly}`"));
                                    break;
                                case FormatType.TimeSpan:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.TimeSpan}`"));
                                    break;
                                case FormatType.TimeOnly:
                                    isChanged = true;
                                    await Pipe.Output.WriteAsync(Encoding.UTF8.GetBytes($"ui.key{keyholeAfter.Key}.nodeValue=`{keyholeAfter.TimeOnly}`"));
                                    break;
                            }
                        }
                    }
                }

                // TODO: State invalidations will not live here
                if (isChanged)
                    await window.DebugSnapshot(Pipe.Output);
            }
            else
            {
                // TODO: Interesting consideration.  What if it's gone?  
                // This is possible by race condition as messages pass 
                // each other across the network.
                Console.WriteLine($"Event handler not found for key:{key}");
            }
        }
    }

    public static (string?, Event?) ParseEvent(ReadOnlySpan<byte> buffer)
    {
        try
        {
            string? key = null;
            Event? @event = null;
            string? method = null;
            ReadOnlySpan<byte> contentType = null;
            int? contentLength = null;

            var lines = buffer.Split((byte)'\n');
            var state = STATE_READ_METHOD;
            foreach (var range in lines)
            {
                var line = buffer[range];
                switch (state)
                {
                    case STATE_READ_METHOD:
                        switch (true)
                        {
                            case bool b when line.StartsWith("CALL "u8):
                                method = "CALL";
                                key = GetKey(line);
                                state = STATE_READ_HEADERS;
                                continue; // parse next line
                            case bool b when line.StartsWith("SET "u8):
                                method = "SET";
                                key = GetKey(line);
                                state = STATE_READ_HEADERS;
                                continue; // parse next line
                        }
                        // Fail if the first line doesn't include a supported method.
                        state = STATE_COMPLETED;
                        break;

                    case STATE_READ_HEADERS:
                        var pair = line.Split((byte)':');
                        if (pair.MoveNext())
                        {
                            var headerName = line[pair.Current];
                            var headerLength = headerName.Length;
                            switch (true)
                            {
                                case bool b when Ascii.EqualsIgnoreCase(headerName, "Content-Length"u8):
                                    if (pair.MoveNext())
                                    {
                                        var value = line[pair.Current];
                                        while (value.Length > 0 && value[0] == (byte)' ')
                                            value = value[1..]; // TrimStart
                                        contentLength = int.Parse(value);
                                    }
                                    break;
                                case bool b when Ascii.EqualsIgnoreCase(headerName, "Content-Type"u8):
                                    if (pair.MoveNext())
                                    {
                                        var value = line[pair.Current];
                                        while (value.Length > 0 && value[0] == (byte)' ')
                                            value = value[1..]; // TrimStart
                                        contentType = value;
                                    }
                                    break;
                                case bool b when headerLength == 0:
                                    state = (contentLength > 0)
                                        ? STATE_READ_BODY
                                        : STATE_COMPLETED;
                                    continue; // parse next line
                            }
                        }
                        break;

                    case STATE_READ_BODY:
                        // Currently, the only valid type for CALL is application/json.
                        if (
                            method == "CALL" &&
                            contentLength is int length && 
                            Ascii.EqualsIgnoreCase(contentType, "application/json"u8))
                        {
                            var bodyStart = range.Start.Value;
                            var bodyEnd = bodyStart + length;
                            if (bodyEnd <= buffer.Length)
                            {
                                var body = buffer[bodyStart..bodyEnd];
                                @event = JsonSerializer.Deserialize<HttpXEvent>(body, JsonSerializerOptions.Web);
                            }
                        }
                        state = STATE_COMPLETED;
                        break;
                    
                    case STATE_COMPLETED:
                        break;
                }
            }
            return (key, @event);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return (null, null);
    }

    private static string? GetKey(ReadOnlySpan<byte> line)
    {
        var keyStart = line.IndexOf("#key"u8);
        if (keyStart < 0)
            return null;
        keyStart += 4;
        var keyEnd = line[keyStart..].IndexOf((byte)' ');
        return Keymaker.GetKeyIfCached(line[keyStart..(keyStart+keyEnd)]);
    }
}