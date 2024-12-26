using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Xui.Web;
using Xui.Web.HttpX.Composers;

namespace Xui.Web.HttpX;

public struct HttpXContext(WebSocketPipe? pipe)
{
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

    public async Task ListenForEvents(Func<Html> html, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(Pipe);

        await Task.WhenAll(
            Receive(html), 
            Pipe.RunAsync(cancellationToken)
        );
    }

    private async Task Receive(Func<Html> html)
    {
        if (Pipe is null)
            return;

        while (await Pipe.Input.ReadAsync() is var result && !result.IsCompleted)
        {
            // TODO: Buffer might be multiple segments one day.
            var buffer = result.Buffer.First;
            var (key, domEvent) = ParseEvent(buffer.Span);
            Pipe.Input.AdvanceTo(result.Buffer.End);

            if (html.GetKeyhole(key) is Func<Event?, Task> eventHandler)
            {
                EventPump.Enqueue(eventHandler, domEvent);
            }
            else
            {
                // TODO: Interesting consideration.  What if it's gone?  
                // This is possible by race condition as messages pass 
                // each other across the network.
            }
        }
    }

    public static (int, Event?) ParseEvent(ReadOnlySpan<byte> buffer)
    {
        int i = 0, key = 0;
        while (true)
        {
            if (i >= buffer.Length)
            {
                return (key, null);
            }

            // Convert from ASCII to int, digit by digit.
            int d = buffer[i] - 48;
            if (d >= 0 && d <= 9)
            {
                key = key * 10 + d;
                ++i;
                continue;
            }
            
            var message = buffer[i..];
            var @event = JsonSerializer.Deserialize<Event>(message);
            return (key, @event);
        }
    }
}