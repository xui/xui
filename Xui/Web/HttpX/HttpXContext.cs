using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Xui.Web;

namespace Xui.Web.HttpX;

public struct HttpXContext(WebSocketPipe? pipe)
{
    public WebSocketPipe? Pipe { get; set; } = pipe;
    public readonly bool IsWebSocketOpen => Pipe is not null && Pipe.State == WebSocketState.Open;

    public static HttpXContext Get(HttpContext httpContext)
    {
        // TODO: Move to header approach?
        var key = httpContext.Connection.Id;
        return new HttpXContext(WebSocketPipe.Get(key));
    }

    public async readonly Task UpdatePath(PathString path)
    {
        if (Pipe is not null && Pipe.State == WebSocketState.Open)
        {
            var writer = Pipe.Output;
            writer.WriteStringLiteral("window.history.pushState({},'', '");
            Encoding.UTF8.GetBytes(path.ToUriComponent().AsSpan(), writer);
            writer.WriteStringLiteral("')");
            await writer.FlushAsync();
        }
    }

    public async Task AwaitEventListeners(HtmlDelegate html, CancellationToken cancellationToken)
    {
        if (Pipe is null)
            return;

        await Task.WhenAll(
            Receive(html), 
            Pipe.RunAsync(cancellationToken)
        );
    }

    private async Task Receive(HtmlDelegate html)
    {
        if (Pipe is null)
            return;

        while (await Pipe.Input.ReadAsync() is var result && !result.IsCompleted)
        {
            // TODO: Buffer might be multiple segments one day.
            var buffer = result.Buffer.First;
            var (slotId, domEvent) = Event.ParseEvent(buffer.Span);
            Pipe.Input.AdvanceTo(result.Buffer.End);

            var eventHandler = GetEventHandlerById(slotId, html);
            EventLoop.Enqueue(eventHandler);
        }
    }

    private static Func<Event, Task> GetEventHandlerById(int slotId, HtmlDelegate html)
    {
        return null;
    }
}