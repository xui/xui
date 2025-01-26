using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Web4;
using Web4.Composers;

namespace Web4.HttpX;

public abstract partial class UI<T> where T : IViewModel
{
    public class HttpXContext
    {
        private readonly UI<T> ui;
        public T ViewModel { get; init; }
        private WebSocketPipe? pipe;
        internal WebSocketPipe? Pipe { get => pipe; }
        private StreamingComposer composer;
        // private Composer composerCompare = new();
        public bool IsWebSocketOpen => pipe?.State == WebSocketState.Open;

        private static readonly MemoryCache cache = new(new MemoryCacheOptions());
        private static readonly MemoryCacheEntryOptions entryOptions =
            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));

        static int tmpCountSessions = 0; // TODO: Remove this after benchmarking is under control.
        public static HttpXContext Get(HttpContext httpContext, UI<T> ui)
        {
            var sessionId = httpContext.Connection.Id;  // TODO: Remove this after benchmarking is under control.
            // var sessionId = httpContext.GetHttpXSessionId();
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

        internal async Task WriteResponseAsync(HttpContext httpContext)
        {
            var pipeWriter = httpContext.Response.BodyWriter;
            composer ??= new DefaultComposer(pipeWriter);
            pipeWriter.Write(composer, $"{ui.MainLayout(ViewModel)}");
            await pipeWriter.FlushAsync();
        }

        private void Precompose()
        {

        }

        private void Recompose()
        {
            // using (composerCompare.ReuseBuffer())
            // {
            //     var html = ui.MainLayout(ViewModel);
            // }
        }

        private async Task Recompose(WebSocketPipe pipe)
        {
            // Recompose();

            // if (!IsWebSocketOpen)
            //     return;

            // var deltas = composer.GetDeltas(composerCompare);
            // if (deltas is null)
            //     return;

            // var writer = pipe.Output;
            // writer.Write(deltas);
            // await writer.FlushAsync();

            // // Swap buffers.
            // (composer, composerCompare) = (composerCompare, composer);
        }

        internal async Task AssignWebSocket(WebSocketManager webSocketManager, HttpContext httpContext)
        {
            using (pipe = await WebSocketPipe.Upgrade(httpContext))
            {
                // HotReload is a no-op in RELEASE mode.
                using (HotReload.Listen(async () => await Recompose(this.pipe)))
                {
                    // TODO: This is almost correct.  
                    // Works across multiple browsers but multiple tabs gets its Action stolen.
                    // Rework this once you figure out the various ViewModel state levels.
                    
                    // ViewModel.OnChanging = async () => await Precompose();
                    ViewModel.OnChanged = async () => await Recompose(pipe);

                    await Task.WhenAll(
                        Receive(pipe), 
                        pipe.RunAsync(httpContext.RequestAborted)
                    );
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
                var (slotId, domEvent) = Web4.HttpX.HttpXContext.ParseEvent(buffer.Span);
                using (this.ViewModel.Batch())
                {
                    //composer.HandleEvent(slotId, domEvent);
                }

                pipe.Input.AdvanceTo(result.Buffer.End);
            }
        }
    }
}