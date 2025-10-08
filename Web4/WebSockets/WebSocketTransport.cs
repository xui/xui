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
        const int RECEIVE_BUFFER_LENGTH = 4096;
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

                case var method when method.EndsWith("dispatchEvent"u8):
                    {
                        var eventSequence       = @params.GetNextAsSequence();
                        currentPropagationID    = @params.GetNextAsInt();
                        currentPropagationLevel = @params.GetNextOptionalAsInt() ?? 0;

                        // Do not handle this event if `stopPropagation()` or `stopImmediatePropagation()`
                        // has previously been called on the browser's same event instance.
                        if (currentPropagationID == suppressPropagationID && currentPropagationLevel >= suppressPropagationLevel)
                            return sequence;

                        DispatchEvent(sequence, eventSequence, GetKey(method));

                        // Return nothing.  DispatchEvent is responsible for returning the buffer(s) to the pool.
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

    private static ReadOnlySpan<byte> GetKey(ReadOnlySpan<byte> method)
    {
        var split = method.Split((byte)'.');
        return split.MoveNext() && split.MoveNext() && split.MoveNext()
            ? method[split.Current]
            : [];
    }

    private void DispatchEvent(ReadOnlySequence<byte> sequence, ReadOnlySequence<byte> eventSequence, ReadOnlySpan<byte> key)
    {
        var eventListener = windowBuilder.GetEventListener(key);

        if (eventListener.Action is Action listener)
        {
            using var batchOutput = Output.UseBatchForThisScope();

            // Done with buffer(s), return to pool early.
            sequence.ReturnToPool();

            Keyholes.DispatchEvent(listener);
        }
        else if (eventListener.ActionEvent is Action<Event> listenerWithEvent)
        {
            using var batchOutput = Output.UseBatchForThisScope();

            Keyholes.DispatchEvent(
                listenerWithEvent,
                new LazyEvent(sequence, eventSequence, this)
                // LazyEvent will return buffer(s) to the pool after it completes.
            );
        }
        else if (eventListener.Func is Func<Task> listenerAsync)
        {
            using var batchOutput = Output.UseBatchForThisScope(continueOnCapturedContext: true);

            // Done with buffer(s), return to pool early.
            sequence.ReturnToPool();

            // Do not await event listeners here!  That would block the WebSocket reader.
            _ = Keyholes.DispatchEvent(listenerAsync);
        }
        else if (eventListener.FuncEvent is Func<Event, Task> listenerWithEventAsync)
        {
            using var batchOutput = Output.UseBatchForThisScope(continueOnCapturedContext: true);

            // Do not await event listeners here!  That would block the WebSocket reader.
            _ = Keyholes.DispatchEvent(
                listenerWithEventAsync,
                new LazyEvent(sequence, eventSequence, this)
                // LazyEvent will return buffer(s) to the pool after it completes.
            );
        }
        else
        {
            sequence.ReturnToPool();
            System.Console.WriteLine($"🛑 Event listener not found for key: `{System.Text.Encoding.UTF8.GetString(key)}`.  Possible race condition.");
        }
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