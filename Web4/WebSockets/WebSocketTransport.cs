using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Web4.Core.DOM;
using Web4.JsonRpc;

namespace Web4.WebSockets;

partial class WebSocketTransport : IWeb4Transport
{
    private static readonly ConcurrentDictionary<string, Web4App> apps = [];
    private readonly WindowBuilder windowBuilder;
    private int currentPropagationID = 0;
    private int currentPropagationLevel = 0;
    private int suppressPropagationID = 0;
    private int suppressPropagationLevel = 0;
    private readonly Channel<ReadOnlySequence<byte>> channel = Channel.CreateBounded<ReadOnlySequence<byte>>(
        options: new BoundedChannelOptions(1000) // TODO: Make this limit configurable?
        {
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
        },
        itemDropped: sequence =>
        {
            // TODO: WebSocket is borked?  Handle:
            // - Skip the line and gracefully close it.
            // - Signal this transport to unregister itself everywhere.
        });

    public JsonRpcWriter Output => JsonRpcWriter.Current(channel.Writer);
    public Web4App App { get; init; }
    public IWindow Window => this;
    public IDocument Document => this;
    public IConsole Console => this;
    public IKeyholes Keyholes => this;

    private WebSocketTransport(string windowID, WindowBuilder windowBuilder, CancellationToken cancel)
    {
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

        var sendTask = transport.ChannelToWebSocket(webSocket, http.RequestAborted);
        var recvTask = transport.WebSocketToTransport(webSocket, http.RequestAborted);
        await Task.WhenAny(sendTask, recvTask);
        
        cancelProcessRegistration.Unregister();
    }

    private async Task ChannelToWebSocket(WebSocket webSocket, CancellationToken cancel)
    {
        while (!cancel.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var sequence = await channel.Reader.ReadAsync(cancel);
                var bytesRemaining = sequence.Length;
                foreach (var memory in sequence)
                {
                    bytesRemaining -= memory.Length;
                    await webSocket.SendAsync(
                        buffer: memory,
                        messageType: WebSocketMessageType.Text,
                        endOfMessage: bytesRemaining == 0,
                        cancellationToken: cancel
                    );
                }

                sequence.ReturnToPool();
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
                    // TODO: new SequenceSegment() allocates memory
                    var segmentStart = new SequenceSegment<byte>(buffer, 0..result.Count);
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

            if (HandleJsonRpcMessage(sequence) is ReadOnlySequence<byte> toReturn)
                toReturn.ReturnToPool();
        }
    }

    private ReadOnlySequence<byte>? HandleJsonRpcMessage(ReadOnlySequence<byte> sequence)
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
                    {
                        using var batchOutput = Output.UseBatchForThisScope();
                        Keyholes.Dump();
                        break;
                    }

                case var method when method.SequenceEqual("app.benchmark"u8):
                    {
                        using var batchOutput = Output.UseBatchForThisScope();
                        var threads = @params.GetNextOptionalAsInt();
                        App.Benchmark(threads);
                        break;
                    }

                case var method when method.SequenceEqual("app.ping"u8) && rpc.IdAsInt is int id:
                    {
                        App.Ping();
                        Output.WriteResponse(id);
                        break;
                    }

                case var method when method.SequenceEqual("app.dispatchEvent"u8):
                    {
                        var eventSequence               = @params.GetNextAsSequence();
                        var key                         = @params.GetNextAsSpan(); // TODO: Move to method path?
                        this.currentPropagationID       = @params.GetNextAsInt();
                        this.currentPropagationLevel    = @params.GetNextOptionalAsInt() ?? 0;

                        if (currentPropagationID == suppressPropagationID && currentPropagationLevel >= suppressPropagationLevel)
                            return sequence;

                        var @event = new LazyEvent(sequence, eventSequence, this);
                        var listener = windowBuilder.GetEventListener(key);

                        if (listener.Action is Action noEventSync)
                        {
                            using var batchOutput = Output.UseBatchForThisScope();
                            @event.Dispose(); // Event is not being used, dispose early.
                            App.DispatchEvent(noEventSync);
                        }
                        else if (listener.ActionEvent is Action<Event> withEventSync)
                        {
                            using var batchOutput = Output.UseBatchForThisScope();
                            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
                            App.DispatchEvent(withEventSync, @event);
                            @event.Dispose();
                        }
                        else if (listener.Func is Func<Task> noEventSyncAsync)
                        {
                            using var batchOutput = Output.UseBatchForThisScope(continueOnCapturedContext: true);
                            @event.Dispose(); // Event is not being used, dispose early.
                            _ = App.DispatchEvent(noEventSyncAsync);
                        }
                        else if (listener.FuncEvent is Func<Event, Task> withEventAsync)
                        {
                            using var batchOutput = Output.UseBatchForThisScope(continueOnCapturedContext: true);
                            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
                            _ = App.DispatchEvent(withEventAsync, @event)
                                .ContinueWith(t => t.Result.Dispose());
                        }
                        else
                        {
                            throw new InvalidOperationException("🛑 No event listener to invoke.  You need to investigate this.");
                        }

                        // Do not return the sequence.  LazyEvent will be responsible for returning the 
                        // buffer(s) to the pool when the event handler is done using the event.
                        return null;
                    }

                default:
                    throw new InvalidOperationException("🛑 Unrecognized method.  This requires investigation.");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"🛑 Error handling JsonRpc message:\n{ex}");
        }
        finally
        {
            // TODO: This doesn't belong here.
            foreach (var a in apps.Values)
                a.Update();
        }
        return sequence;
    }

    public void Diff(Keyhole[] oldBuffer, Keyhole[] newBuffer)
    {
        using var batchOutput = Output.UseBatchForThisScope();
        DiffUtil.Diff(this, oldBuffer, newBuffer);
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
}