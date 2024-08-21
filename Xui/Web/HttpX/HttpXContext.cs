using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Xui.Web;

namespace Xui.Web.HttpX;

public abstract partial class UI<T> where T : IViewModel
{
    public class HttpXContext
    {
        private readonly UI<T> ui;
        public T ViewModel { get; init; }
        private WebSocketPipe? pipe;
        private Composer composer = new();
        private Composer composerCompare = new();
        public bool IsWebSocketOpen => pipe?.State == WebSocketState.Open;

        private static readonly MemoryCache cache = new(new MemoryCacheOptions());
        private static readonly MemoryCacheEntryOptions entryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        static int tmpCountSessions = 0; // TODO: Remove this after benchmarking is under control.
        public static HttpXContext Get(HttpContext httpContext, UI<T> ui)
        {
            // var sessionId = httpContext.Connection.Id;  // TODO: Remove this after benchmarking is under control.
            var sessionId = httpContext.GetHttpXSessionId();
            if (cache.Get(sessionId) is not HttpXContext context)
            {
                Console.WriteLine($"Context: {tmpCountSessions++} {httpContext.Connection.Id}");
                context = new HttpXContext(ui);
                Set(sessionId, context);
            }
            return context;
        }

        private static void Set(string id, HttpXContext context)
        {
            cache.Set(id, context, entryOptions);
        }

        public HttpXContext(UI<T> ui)
        {
            this.ui = ui;

            // TODO: Need to be more clever about how a new ViewModel is created.
            ViewModel = (T)T.New();
        }

        public void Compose()
        {
            using (composer.ReuseBuffer())
            {
                var html = ui.MainLayout(ViewModel);
            }
        }

        internal async Task WriteResponseAsync(HttpContext httpContext)
        {
            Compose();

            var contentLength = composer.GetContentLengthIfConvenient();
            if (contentLength.HasValue)
                httpContext.Response.ContentLength = contentLength.Value;

            var pipeWriter = httpContext.Response.BodyWriter;
            pipeWriter.Write(composer.AsSpan());
            await pipeWriter.FlushAsync();
        }

        internal async Task Recompose(WebSocketPipe pipe)
        {
            using (composerCompare.ReuseBuffer())
            {
                var html = ui.MainLayout(ViewModel);
            }

            if (!IsWebSocketOpen)
                return;

            var deltas = composer.GetDeltas(composerCompare);
            if (deltas is null)
                return;

            var writer = pipe.Output;
            writer.Write(deltas);
            await writer.FlushAsync();

            // Swap buffers.
            (composer, composerCompare) = (composerCompare, composer);
        }

        internal async Task PushHistoryState(string path)
        {
            if (IsWebSocketOpen && pipe is not null)
            {
                var writer = pipe.Output;
                writer.WriteStringLiteral($"window.history.pushState({{}},'', '{path}')");
                writer.Write(path);
                writer.WriteStringLiteral("')");
                await writer.FlushAsync();
            }
        }

        internal async Task AssignWebSocket(WebSocketManager webSocketManager, HttpContext httpContext)
        {
            using (var webSocket = await webSocketManager.AcceptWebSocketAsync())
            {
                using (pipe = new WebSocketPipe(webSocket))
                {
                    // HotReload is a no-op in RELEASE mode.
                    using (HotReload.Listen(async () => await Recompose(this.pipe)))
                    {
                        // TODO: This is almost correct.  
                        // Works across multiple browsers but multiple tabs gets its Action stolen.
                        // Rework this once you figure out the various ViewModel state levels.
                        ViewModel.OnChanged = async () => await Recompose(pipe);

                        await Task.WhenAll(
                            Receive(pipe), 
                            pipe.RunAsync(httpContext.RequestAborted)
                        );
                    }
                }
            }

            pipe = null;
            // TODO: Should ViewModel.OnChange unsubscribe here?
        }

        private async Task Receive(IDuplexPipe pipe)
        {
            while (await pipe.Input.ReadAsync() is var result && !result.IsCompleted)
            {
                // TODO: Buffer might be multiple segments one day.
                var buffer = result.Buffer.First;
                var (slotId, domEvent) = ParseEvent(buffer.Span);
                using (this.ViewModel.Batch())
                {
                    composer.HandleEvent(slotId, domEvent);
                }

                pipe.Input.AdvanceTo(result.Buffer.End);
            }
        }

        private (int, Event?) ParseEvent(ReadOnlySpan<byte> buffer)
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

                if (i >= buffer.Length - 1)
                    return (slot, null);
                
                var message = buffer[i..];
                var @event = JsonSerializer.Deserialize<Event>(message);
                return (slot, @event);
            }
        }
    }
}