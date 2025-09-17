using System.Diagnostics;
using System.Threading.Channels;
using Web4.Composers;

namespace Web4;

public class Web4App
{
    public static SnapshotStrategy SnapshotStrategy { get; set; } = SnapshotStrategy.Retain;
    public bool IsInvalidated { get; private set; } = false;
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMilliseconds(1000d / 60d); // 60fps
    internal IWeb4Transport Transport { get; init; }
    private readonly WindowBuilder windowBuilder;
    private readonly Channel<int> updateDebouncer;
    private Keyhole[]? snapshot = null;

    public Web4App(IWeb4Transport transport, WindowBuilder windowBuilder, CancellationToken cancel)
    {
        this.Transport = transport;
        this.windowBuilder = windowBuilder;

        // This channel has a max capacity of 1 and is configured to 
        // drop subsequent update-requests when it is full.
        // This allows it to support debouncing, prevent concurrent mutations, and 
        // will not forget update-requests made within the debouncing window.
        updateDebouncer = Channel.CreateBounded<int>(new BoundedChannelOptions(1)
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });

        Task.Run(async () =>
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
                var timeUntilNextUpdate = UpdateInterval.Subtract(lastUpdate.Elapsed);
                if (timeUntilNextUpdate > TimeSpan.Zero)
                    await Task.Delay(timeUntilNextUpdate, cancel);
                lastUpdate.Restart();

                // If here, this app has a green light to do work again 
                // so await until an update is requested.  
                _ = await updateDebouncer.Reader.ReadAsync(cancel);
                await UpdateAsync();
            }
        }, cancel);
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

        while (!updateDebouncer.Writer.TryWrite(0)) ;
    }

    private async ValueTask UpdateAsync()
    {
        if (!IsInvalidated)
        {
            Console.WriteLine($"Cancelling Update() this app has not been invalidated.");
            return;
        }

        if (snapshot is null)
        {
            Console.WriteLine($"Cancelling Update() because snapshot is null (which is unexpected and should be investigated).");
            return;
        }

        Keyhole[] oldBuffer = snapshot;
        Keyhole[] newBuffer = CaptureSnapshot();

        await Transport.ApplyMutations(oldBuffer, newBuffer);

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

    public void Benchmark(int? threads)
    {
    }

    public void DispatchEvent<T>(T @event, string key, int propagationID) where T : struct, Event
    {
        try
        {
            var listener = windowBuilder.GetEventListener(key);

            using var perf = Debug.PerfCheck("DispatchEvent"); // TODO: Remove PerfCheck

            // TODO: Set window-specific values here?
            // event.View = app.Window;
            // event.PropagationID = propagationID; // Is this WebSocket-specific?
            // component.Window = app.Window;

            var found = listener switch
            {
                _ when listener.Action is Action noEventSync => HandleEventListener(noEventSync, @event),
                _ when listener.ActionEvent is Action<Event> withEventSync => HandleEventListener(withEventSync, @event),
                _ when listener.Func is Func<Task> noEventAsync => HandleEventListener(noEventAsync, @event),
                _ when listener.FuncEvent is Func<Event, Task> withEventAsync => HandleEventListener(withEventAsync, @event),
                _ => false
            };

            if (found)
                Update();
            else
                Console.WriteLine("🔴 No event listener to invoke.  You need to investigate this.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private bool HandleEventListener<T>(Action listener, T @event) where T : struct, Event
    {
        @event.Dispose(); // Event is not being used, dispose early.
        listener();
        return true;
    }

    private bool HandleEventListener<T>(Action<Event> listener, T @event) where T : struct, Event
    {
        // TODO: Memory allocation casting from TEvent (struct) to Event interface.
        listener(@event);
        @event.Dispose();
        return true;
    }

    private bool HandleEventListener<T>(Func<Task> listener, T @event) where T : struct, Event
    {
        @event.Dispose(); // Event is not being used, dispose early.

        // TODO: Fix memory allocation
        // TODO: Can this be WebSocket only please?
        // SynchronizationContext.SetSynchronizationContext(new EventListenerSynchronizationContext(app));
        listener();
        // SynchronizationContext.SetSynchronizationContext(null);
        return true;
    }

    private bool HandleEventListener<T>(Func<Event, Task> listener, T @event) where T : struct, Event
    {
        // TODO: Fix memory allocation
        // TODO: Can this be WebSocket only please?
        // SynchronizationContext.SetSynchronizationContext(new EventListenerSynchronizationContext(app));
        // TODO: Memory allocation casting from TEvent (struct) to Event interface.
        listener(@event)
            // TODO: Memory alloation from capturing.
            .ContinueWith(t => @event.Dispose());
        // SynchronizationContext.SetSynchronizationContext(null);
        return true;
    }

    public void DumpKeyholes(System.Net.WebSockets.WebSocket webSocket) // TODO: Remove webSocket once ConsoleProxy is in place.
    {
        var snapshot = this.snapshot ?? CaptureSnapshot();
        Debug.Dump(webSocket, snapshot);
    }

    public void Ping()
    {
        // no-op
    }

    public void SetRefreshRate(int milliseconds)
    {
        UpdateInterval = TimeSpan.FromMilliseconds(milliseconds);
    }

    private Keyhole[] CaptureSnapshot()
    {
        using var perf = Debug.PerfCheck("CaptureSnapshot"); // TODO: Remove PerfCheck

        return windowBuilder.Html.CreateSnapshot();
    }

}
