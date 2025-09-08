using System.Diagnostics;
using System.Threading.Channels;
using Web4.Composers;

namespace Web4;

public class Window
{
    private readonly IWeb4Transport transport;
    private readonly WindowBuilder windowBuilder;
    private readonly Channel<int> updateDebouncer;
    private Keyhole[]? snapshot = null;

    public static SnapshotStrategy SnapshotStrategy { get; set; } = SnapshotStrategy.Retain;
    public bool IsInvalidated { get; private set; } = false;
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromMilliseconds(1000d / 60d); // 60fps

    public Window(IWeb4Transport transport, WindowBuilder windowBuilder, CancellationToken cancel)
    {
        this.transport = transport;
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
        ReadFromUpdateDebouncer(cancel);
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

    public void RequestUpdate()
    {
        if (!IsInvalidated)
            return;

        while (!updateDebouncer.Writer.TryWrite(0)) ;
    }

    public async Task Disconnect()
    {
        await transport.Disconnect();
    }

    private void ReadFromUpdateDebouncer(CancellationToken cancel)
    {
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

                // If here, this window has a green light to do work again 
                // so await until an update is requested.  
                _ = await updateDebouncer.Reader.ReadAsync(cancel);
                await Update();
            }
        }, cancel);
    }

    private async ValueTask Update()
    {
        if (!IsInvalidated)
        {
            Console.WriteLine($"Cancelling Update() this window has not been invalidated.");
            return;
        }

        if (snapshot is null)
        {
            Console.WriteLine($"Cancelling Update() because snapshot is null (which is unexpected and should be investigated).");
            return;
        }

        Keyhole[] oldBuffer = snapshot;
        Keyhole[] newBuffer = CaptureSnapshot();

        await transport.ApplyMutations(oldBuffer, newBuffer);

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

    private Keyhole[] CaptureSnapshot()
    {
        using var perf = Debug.PerfCheck("CaptureSnapshot"); // TODO: Remove PerfCheck

        return windowBuilder.Html.CreateSnapshot();
    }

    internal void HandleEvent<T>(string key, ref T @event) where T : struct, Event
    {
        try
        {
            var listener = windowBuilder.GetEventListener(key);

            // Invoke the proper method signature.  Important notes:
            // - it might not pass in the event, e.g. `void OnClick()`
            // - it may or may not be async but clearly does not await here
            // - it disposes the event after the method completes
            listener.Invoke(@event);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
