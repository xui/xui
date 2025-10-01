using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Web4.Core.DOM;
using Web4.JsonRpc;

namespace Web4.WebSockets;

partial class WebSocketTransport : IWeb4Transport
{
    private static readonly ConcurrentDictionary<string, Web4App> apps = [];
    private readonly Pipe pipe = new();
    private readonly WindowBuilder windowBuilder;
    private EventListenerSynchronizationContext? syncContext = null;
    private int currentPropagationID = 0;
    private int currentPropagationLevel = 0;
    private int suppressPropagationID = 0;
    private int suppressPropagationLevel = 0;

    public JsonRpcWriter Output { get; init; }
    public Web4App App { get; init; }
    public IWindow Window => this;
    public IDocument Document => this;
    public IConsole Console => this;
    public IKeyholes Keyholes => this;

    private WebSocketTransport(string windowID, WindowBuilder windowBuilder, CancellationToken cancel)
    {
        this.Output = new JsonRpcWriter(pipe.Writer);
        this.windowBuilder = windowBuilder;

        if (!apps.TryGetValue(windowID, out var app))
        {
            app = new Web4App(this, windowBuilder, cancel);
            apps[windowID] = app;
        }
        App = app;
    }

    public static async Task Bind(HttpContext http, WindowBuilder windowBuilder, CancellationToken cancelProcess)
    {
        // TODO: Move to header approach?
        var windowID = http.Connection.Id;
        var transport = new WebSocketTransport(windowID, windowBuilder, cancelProcess);

        // TODO: Move to config
        var context = new WebSocketAcceptContext
        {
            KeepAliveInterval = TimeSpan.FromSeconds(60),
            KeepAliveTimeout = TimeSpan.FromSeconds(20)
        };
        using var webSocket = await http.WebSockets.AcceptWebSocketAsync(context);
        var cancelProcessRegistration = cancelProcess.Register(async () => await Disconnect(webSocket));

        var sendTask = transport.PipeToWebSocket(webSocket, transport.pipe.Reader, http.RequestAborted);
        var recvTask = transport.WebSocketToTransport(webSocket, http.RequestAborted);
        await Task.WhenAny(sendTask, recvTask);
        
        cancelProcessRegistration.Unregister();
    }

    public void Flush()
    {
        var flushTask = Output.Flush();
    }

    private async Task PipeToWebSocket(WebSocket webSocket, PipeReader reader, CancellationToken cancel)
    {
        while (!cancel.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var jsonRpcBatch = await reader.ReadAsync(cancel);
                var bytesRemaining = jsonRpcBatch.Buffer.Length;
                foreach (var memory in jsonRpcBatch.Buffer)
                {
                    bytesRemaining -= memory.Length;
                    await webSocket.SendAsync(
                            buffer: memory,
                            messageType: WebSocketMessageType.Text,
                            endOfMessage: bytesRemaining == 0,
                            cancellationToken: cancel
                        );
                }
                reader.AdvanceTo(jsonRpcBatch.Buffer.End);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"🛑 Unexpected error writing to the WebSocket.\n{ex}");
            }
        }
    }

    private async Task WebSocketToTransport(WebSocket webSocket, CancellationToken cancel)
    {
        const int RECEIVE_BUFFER_LENGTH = 1024;
        ReadOnlySequence<byte> sequence;
        while (true)
        {
            try
            {
                var buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
                var result = await webSocket.ReceiveAsync(buffer, cancel);

                System.Console.WriteLine();
                using var perf = Debug.PerfCheck("WebSocketToTransport (loop)"); // TODO: Remove PerfCheck

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
                    // TODO: This jank is motivation to move to an IDuplexPipe with a custom reader that defers buffer-return-responsibilities?
                    var segmentStart = new Segment(buffer, 0..result.Count);
                    var segmentEnd = segmentStart;
                    while (!result.EndOfMessage)
                    {
                        buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
                        result = await webSocket.ReceiveAsync(buffer, cancel);
                        segmentEnd = segmentEnd.Append(buffer, 0..result.Count);
                    }
                    sequence = new ReadOnlySequence<byte>(segmentStart, 0, segmentEnd, result.Count);
                }
            }
            catch (WebSocketException)
            {
                break;
            }

            if (HandleJsonRpcMessage(sequence))
            {
                await Output.Flush(cancel);
            }
        }
    }

    private bool HandleJsonRpcMessage(ReadOnlySequence<byte> sequence)
    {
        // TODO: This doesn't belong here.
        foreach (var a in apps.Values)
            a.Invalidate();

        try
        {
            var rpc = new JsonRpcReader(sequence);
            var @params = rpc.ReadPositionalParams();

            switch (rpc.Method)
            {
                case var method when method.SequenceEqual("app.keyholes.dump"u8):
                    Keyholes.Dump();
                    break;

                case var method when method.SequenceEqual("app.benchmark"u8):
                    var threads = @params.GetNextOptionalAsInt();
                    App.Benchmark(threads);
                    break;

                case var method when method.SequenceEqual("app.ping"u8) && rpc.IdAsInt is int id:
                    App.Ping();
                    Output.WriteResponse(id);
                    break;

                case var method when method.SequenceEqual("app.dispatchEvent"u8):
                    var @event = new LazyEvent(@params.GetNextAsSequence(), this);
                    var key = @params.GetNextAsSpan(); // TODO: Move to method path?
                    this.currentPropagationID = @params.GetNextAsInt();
                    this.currentPropagationLevel = @params.GetNextOptionalAsInt() ?? 0;

                    if (currentPropagationID == suppressPropagationID && currentPropagationLevel >= suppressPropagationLevel)
                        return true;

                    var listener = windowBuilder.GetEventListener(key);

                    if (listener.Action is Action noEventSync)
                    {
                        @event.Dispose(); // Event is not being used, dispose early.
                        App.DispatchEvent(noEventSync);
                    }
                    else if (listener.ActionEvent is Action<Event> withEventSync)
                    {
                        // TODO: Memory allocation casting from TEvent (struct) to Event interface.
                        App.DispatchEvent(withEventSync, @event);
                        @event.Dispose();
                    }
                    else if (listener.Func is Func<Task> noEventSyncAsync)
                    {
                        @event.Dispose(); // Event is not being used, dispose early.
                        SynchronizationContext.SetSynchronizationContext(syncContext ??= new(this));
                        _ = App.DispatchEvent(noEventSyncAsync);
                        SynchronizationContext.SetSynchronizationContext(null);
                    }
                    else if (listener.FuncEvent is Func<Event, Task> withEventAsync)
                    {
                        SynchronizationContext.SetSynchronizationContext(syncContext ??= new(this));
                        // TODO: Memory allocation casting from TEvent (struct) to Event interface.
                        _ = App.DispatchEvent(withEventAsync, @event)
                            .ContinueWith(t => t.Result.Dispose());
                        SynchronizationContext.SetSynchronizationContext(null);
                    }
                    else
                    {
                        System.Console.WriteLine("🛑 No event listener to invoke.  You need to investigate this.");
                        return false;
                    }
                    break;

                default:
                    System.Console.WriteLine("🛑 Unrecognized method.  This requires investigation.");
                    return false;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"🛑 Error handling JsonRpc message:\n{ex}");
            return false;
        }

        // TODO: This doesn't belong here.
        foreach (var a in apps.Values)
            a.Update();

        return true;
    }

    internal void StopPropagation()
    {
        suppressPropagationID = currentPropagationID;
        suppressPropagationLevel = currentPropagationLevel + 1;
    }

    internal void StopImmediatePropagation()
    {
        suppressPropagationID = currentPropagationID;
        suppressPropagationLevel = 0;
    }

    private static async Task Disconnect(WebSocket webSocket)
    {
        await webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Application ended...",
            CancellationToken.None
        );
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
}