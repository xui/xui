using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Web4.Composers;
using Web4.Core.DOM;
using Web4.JsonRpc;

namespace Web4.WebSockets;

partial class WebSocketTransport
{
    public static SnapshotStrategy SnapshotStrategy { get; set; } = SnapshotStrategy.Retain;
    private static readonly ConcurrentDictionary<string, WebSocketTransport> transports = [];
    private readonly WindowBuilder windowBuilder;
    private TimeSpan diffInterval = TimeSpan.FromMilliseconds(1000d / 60d); // 60fps
    private readonly Channel<int> diffChannel;
    private readonly Channel<ReadOnlySequence<byte>> outputChannel;
    private Keyhole[]? snapshot = null;


    public bool IsInvalidated { get; private set; } = false;
    public Propagation Propagation { get; } = new();
    public JsonRpcWriter Output => JsonRpcWriter.Current(outputChannel.Writer);
    public IWindow Window => this;
    public IDocument Document => this;
    public IConsole Console => this;
    public IKeyholes Keyholes => this;

    private WebSocketTransport(string windowID, WindowBuilder windowBuilder, CancellationToken cancel)
    {
        this.windowBuilder = windowBuilder;

        diffChannel = Channel.CreateBounded<int>(
            new BoundedChannelOptions(1)
            {
                AllowSynchronousContinuations = true,
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropWrite
            }
        );

        outputChannel = Channel.CreateBounded<ReadOnlySequence<byte>>(
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
            }
        );

        // if (!apps.TryGetValue(windowID, out var transport))
        // {
        //     apps[windowID] = app;
        // }
        // App = app;
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

        transports[windowID] = transport;

        await Task.WhenAny(
            transport.OutputToWebSocket(webSocket, http.RequestAborted),
            transport.WebSocketToMethod(webSocket, http.RequestAborted),
            transport.DebounceDiffs(http.RequestAborted)
        );

        System.Console.WriteLine($"👋 Goodbye, `{windowID}`!");

        transports.Remove(windowID, out var _);
        cancelProcessRegistration.Unregister();
    }

    private async Task OutputToWebSocket(WebSocket webSocket, CancellationToken cancel)
    {
        while (!cancel.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var sequence = await outputChannel.Reader.ReadAsync(cancel);
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
            catch (OperationCanceledException)
            {
                System.Console.WriteLine($"Connection severed.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"🛑 Unexpected error writing to the WebSocket.\n{ex}");
            }
        }
    }

    private async Task WebSocketToMethod(WebSocket webSocket, CancellationToken cancel)
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

    private async Task DebounceDiffs(CancellationToken cancel)
    {
        // TODO: I don't like how it awaits Task.Delay on the first run.

        var lastUpdate = Stopwatch.StartNew();
        while (!cancel.IsCancellationRequested)
        {
            // Debounce!  If the last update was less than 16 ms ago (60 fps),
            // then wait until the "next frame" before updating again
            // (i.e. no need to run Update() more frequently than the screen's refresh rate).
            // There could be (potentially) millions of state changes every 16 ms
            // and it'd be wasteful to run diffs or send mutations for each
            // when most screens can only handle 60 Hz.
            var timeUntilNextUpdate = diffInterval.Subtract(lastUpdate.Elapsed);
            if (timeUntilNextUpdate > TimeSpan.Zero)
                await Task.Delay(timeUntilNextUpdate, cancel);
            lastUpdate.Restart();

            // If here, this app has a green light to do work again 
            // so await until an update is requested.  
            _ = await diffChannel.Reader.ReadAsync(cancel);
            Reconcile();
        }
    }

    private ReadOnlySequence<byte>? HandleJsonRpcMessage(ReadOnlySequence<byte> sequence)
    {
        // TODO: This doesn't belong here.
        foreach (var t in transports.Values)
            t.Invalidate();

        try
        {
            var rpc = new JsonRpcReader(sequence);
            var @params = rpc.ReadPositionalParams();

            switch (rpc.Method)
            {
                case var method when method.SequenceEqual("ui.keyholes.dump"u8):
                    {
                        using var batchOutput = Output.UseBatchForThisScope();
                        Keyholes.Dump();
                        break;
                    }

                case var method when method.SequenceEqual("app.benchmark"u8):
                    {
                        using var batchOutput = Output.UseBatchForThisScope();
                        var threads = @params.GetNextOptionalAsInt();
                        // App.Benchmark(threads);
                        break;
                    }

                case var method when method.SequenceEqual("app.ping"u8) && rpc.IdAsInt is int id:
                    {
                        // App.Ping();
                        Output.WriteResponse(id);
                        break;
                    }

                case var method when method.EndsWith("dispatchEvent"u8):
                    {
                        var eventSequence = @params.GetNextAsSequence();
                        Propagation.CurrentID = @params.GetNextAsInt();
                        Propagation.CurrentLevel = @params.GetNextOptionalAsInt() ?? 0;

                        // Do not handle this event if `stopPropagation()` or `stopImmediatePropagation()`
                        // has previously been called on the browser's same event instance.
                        if (Propagation.IsStopped)
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
            foreach (var t in transports.Values)
                t.Update();
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

    private static async Task Disconnect(WebSocket webSocket)
    {
        await webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Application ended...",
            CancellationToken.None
        );
    }

    public void SetRefreshRate(int milliseconds)
    {
        diffInterval = TimeSpan.FromMilliseconds(milliseconds);
    }

    private Keyhole[] CaptureSnapshot()
    {
        using var perf = Debug.PerfCheck("CaptureSnapshot"); // TODO: Remove PerfCheck

        return windowBuilder.Html.CreateSnapshot();
    }

    public void Invalidate()
    {
        if (IsInvalidated)
            return;

        IsInvalidated = true;

        // This only does work when SnapshotStrategy is in "Recapture" mode
        // since snapshot is never set to null while operating in "Retain" mode.
        snapshot ??= CaptureSnapshot();
    }

    public void Update()
    {
        if (!IsInvalidated)
            return;

        while (!diffChannel.Writer.TryWrite(0)) ;
    }

    private void Reconcile()
    {
        if (!IsInvalidated)
        {
            System.Console.WriteLine($"Cancelling Update() this app has not been invalidated.");
            return;
        }

        if (snapshot is null)
        {
            System.Console.WriteLine($"Cancelling Update() because snapshot is null (which is unexpected and should be investigated).");
            return;
        }

        Keyhole[] oldBuffer = snapshot;
        Keyhole[] newBuffer = CaptureSnapshot();
        using (var batchOutput = Output.UseBatchForThisScope())
        {
            DiffUtil.Diff(Keyholes, oldBuffer, newBuffer);
        }

        switch (SnapshotStrategy)
        {
            case SnapshotStrategy.Recapture:
                // Do not keep this snapshot buffer for later.
                snapshot = null;
                oldBuffer.Return();
                newBuffer.Return();
                break;
            case SnapshotStrategy.Retain:
                // Keep this snapshot buffer around to use as the "before" next time.
                snapshot = newBuffer;
                oldBuffer.Return();
                break;
        }

        IsInvalidated = false;
    }
}