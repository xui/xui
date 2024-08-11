using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Xui.Web.Html;

namespace Xui.Web.HttpX;

public abstract partial class UI<T> where T : IViewModel
{
    public class Context
    {
        private readonly UI<T> ui;
        public T ViewModel { get; init; }
        private WebSocket? webSocket;
        private HtmlString htmlString;
        private HtmlString htmlStringCompare;
        private readonly byte[] receiveBuffer = new byte[1024 * 4];
        private readonly byte[] sendBuffer = new byte[1024 * 4];

        private static readonly MemoryCache cache = new(new MemoryCacheOptions());
        private static readonly MemoryCacheEntryOptions entryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        public static Context Get(HttpContext httpContext, UI<T> ui)
        {
            var sessionId = httpContext.GetHttpXSessionId();
            if (cache.Get(sessionId) is not Context context)
            {
                context = new Context(ui);
                Set(sessionId, context);
            }
            return context;
        }

        private static void Set(string id, Context context)
        {
            cache.Set(id, context, entryOptions);
        }

        public bool IsWebSocketOpen
        {
            get => webSocket != null && webSocket.State == WebSocketState.Open;
        }

        public Context(UI<T> ui)
        {
            this.ui = ui;

            htmlString = $"";
            htmlStringCompare = $"";

            // TODO: Need to be more clever about how a new ViewModel is created.
            ViewModel = (T)T.New();
        }

        public void Compose()
        {
            using (htmlString.ReuseBuffer())
            {
                htmlString = ui.MainLayout(ViewModel);
            }
        }

        internal async Task WriteResponseAsync(HttpContext httpContext)
        {
            Compose();

            var contentLength = htmlString.GetContentLengthIfConvenient();
            if (contentLength.HasValue)
                httpContext.Response.ContentLength = contentLength.Value;

            var pipeWriter = httpContext.Response.BodyWriter;
            await pipeWriter.WriteAsync(ref htmlString);
        }

        internal async Task Recompose()
        {
            using (htmlStringCompare.ReuseBuffer())
            {
                htmlStringCompare = ui.MainLayout(ViewModel);
            }

            var deltas = htmlString.GetDeltas(htmlStringCompare);
            await PushMutations(deltas);
        }

        internal async Task PushMutations(IEnumerable<Delta> deltas)
        {
            if (!IsWebSocketOpen)
                return;

            StringBuilder? output = null;
            foreach (var delta in deltas)
            {
                output ??= new();

                switch (delta.Type)
                {
                    case DeltaType.NodeValue:
                        // TODO: Eventually these "eval commands" will already be a part of the DOM.
                        output.Append("slot");
                        output.Append(delta.Id);
                        output.Append(".nodeValue='");
                        output.Append(delta.Output);
                        output.Append("';");
                        break;
                    case DeltaType.NodeAttribute:
                        // TODO: Support this.
                        break;
                    case DeltaType.HtmlPartial:
                        output.Append("replaceNode(slot");
                        output.Append(delta.Id);
                        output.Append(",`");
                        output.Append(delta.Output);
                        output.Append("`);");
                        break;
                }
            }

            // Swap buffers.
            (htmlStringCompare, htmlString) = (htmlString, htmlStringCompare);

            if (output is not null && IsWebSocketOpen)
            {
                await webSocket!.SendAsync(output);
            }
        }

        internal ValueTask PushHistoryState(string path)
        {
            if (IsWebSocketOpen)
            {
                var output = new StringBuilder();
                output.Append($"window.history.pushState({{}},'', '{path}')");
                return webSocket!.SendAsync(output);
            }
            else
            {
                return ValueTask.CompletedTask;
            }
        }

        internal async Task AssignWebSocket(WebSocketManager webSocketManager)
        {
            // TODO: This is almost correct.  Works across multiple browsers but multiple tabs gets its Action stolen.
            // Rework this once you figure out the various ViewModel state levels.
            ViewModel.OnChanged = async () => await Recompose();

#if DEBUG
            using (new HotReloadContext<T>(this))
#endif
            using (var webSocket = await webSocketManager.AcceptWebSocketAsync())
            {
                this.webSocket = webSocket;
                await Receive(webSocket);
            }
        }

        private async Task Receive(WebSocket webSocket)
        {
            WebSocketReceiveResult? receiveResult;
            do
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(receiveBuffer),
                    CancellationToken.None
                );

                if (receiveResult.Count == 0)
                    continue;

                var (slotId, domEvent) = ParseEvent(receiveBuffer, receiveResult.Count);
                using (this.ViewModel.Batch())
                {
                    htmlString.HandleEvent(slotId, domEvent);
                }
            }
            while (!receiveResult.CloseStatus.HasValue);

            Console.WriteLine("Closing the connection...");

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None
            );

            Console.WriteLine("Connection closed.");
        }

        private (int, Event?) ParseEvent(byte[] buffer, int length)
        {
            int i = 0, slot = 0;
            while (true)
            {
                // Convert from ASCII to int, digit by digit.
                int d = buffer[i] - 48;
                if (d >= 0 && d <= 9) {
                    slot = slot * 10 + d;
                    ++i;
                    continue;
                }

                if (i >= length - 1)
                    return (slot, null);
                
                // TODO: Optimize (or hopefully move to SignalR).
                var message = Encoding.UTF8.GetString(buffer, i, length - i);
                var @event = JsonSerializer.Deserialize<Event>(message);
                return (slot, @event);
            }
        }
    }
}