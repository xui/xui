using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Threading.Channels;

namespace Web4;

public class State<T>
{
    public const string GLOBAL_TOPIC = "process://global";

    [ThreadStatic]
    static bool returnInvalidated;
    public static bool ReturnInvalidated { get => returnInvalidated; set => returnInvalidated = value; }

    private readonly static Dictionary<State<T>, T> invalidatedValues = [];

    private T _value;
    public T Value
    {
        get => returnInvalidated ? invalidatedValues.GetValueOrDefault(this, _value) : _value;
        set
        {
            // Don't save the before value if one already exists.
            if (invalidatedValues.TryGetValue(this, out var before))
            {
                // If the value is set to its original, remove its invalidation.
                if (EqualityComparer<T>.Default.Equals(before, value))
                {
                    invalidatedValues.Remove(this);
                }
            }
            else
            {
                // Save the invalidated value.
                invalidatedValues[this] = value;
            }
            
            _value = value;
        }
    }

    public string Topic { get; init; }
    
    internal State(T value, string topic = GLOBAL_TOPIC)
    {
        _value = value;
        Topic = topic;
    }

    public static implicit operator T(State<T> state) => state.Value;

    public static State<T> operator ++(State<T> item)
    {
        return item switch
        {
            State<int> i => item.Increment(i, 1),
            State<long> l => item.Increment(l, 1),
            State<float> f => item.Increment(f, 1),
            State<double> d => item.Increment(d, 1),
            State<decimal> m => item.Increment(m, 1),
            _ => item
        };
    }

    public static State<T> operator --(State<T> item)
    {
        return item switch
        {
            State<int> i => item.Increment(i, -1),
            State<long> l => item.Increment(l, -1),
            State<float> f => item.Increment(f, -1),
            State<double> d => item.Increment(d, -1),
            State<decimal> m => item.Increment(m, -1),
            _ => item
        };
    }

    private State<T> Increment(State<int> s, int increment) { s.Value += increment; return this; }
    private State<T> Increment(State<long> s, long increment) { s.Value += increment; return this; }
    private State<T> Increment(State<float> s, float increment) { s.Value += increment; return this; }
    private State<T> Increment(State<double> s, double increment) { s.Value += increment; return this; }
    private State<T> Increment(State<decimal> s, decimal increment) { s.Value += increment; return this; }
}

public static class StateExtensions
{
    public static State<string> AsState(this string value, string topic = State<string>.GLOBAL_TOPIC) => new(value, topic);
    public static State<bool> AsState(this bool value, string topic = State<bool>.GLOBAL_TOPIC) => new(value, topic);
    public static State<int> AsState(this int value, string topic = State<int>.GLOBAL_TOPIC) => new(value, topic);
    public static State<long> AsState(this long value, string topic = State<long>.GLOBAL_TOPIC) => new(value, topic);
    public static State<float> AsState(this float value, string topic = State<float>.GLOBAL_TOPIC) => new(value, topic);
    public static State<double> AsState(this double value, string topic = State<double>.GLOBAL_TOPIC) => new(value, topic);
    public static State<decimal> AsState(this decimal value, string topic = State<decimal>.GLOBAL_TOPIC) => new(value, topic);
    public static State<DateTime> AsState(this DateTime value, string topic = State<DateTime>.GLOBAL_TOPIC) => new(value, topic);
    public static State<TimeSpan> AsState(this TimeSpan value, string topic = State<TimeSpan>.GLOBAL_TOPIC) => new(value, topic);
}

// public class State
// {
//     private static readonly Thread uiThread;
//     private static readonly Channel<SendOrPostCallback> channel = Channel.CreateUnbounded<SendOrPostCallback>(new UnboundedChannelOptions {
//         SingleWriter = false,
//         SingleReader = true,
//         AllowSynchronousContinuations = false,
//     });

//     public static void Invoke(Action action)
//     {
//         channel.Writer.TryWrite(_ => action());
//     }

//     public static void Invoke(Func<Task> asyncTask)
//     {
//         channel.Writer.TryWrite(_ => asyncTask());
//     }

//     static State()
//     {
//         uiThread = new(Loop) { Name = "UI Thread" };
//         uiThread.Start();
//     }

//     private static async void Loop()
//     {
//         Console.WriteLine("--- Hello from the UI Thread! ---");

//         var syncContext = new UISynchronizationContext();
//         SynchronizationContext.SetSynchronizationContext(syncContext);

//         long batch = 0;

//         await foreach (SendOrPostCallback callback in channel.Reader.ReadAllAsync())
//         {
//             Console.WriteLine("Thanks for the action.  Executing now...");

//             // Mutate state
//             callback(null);

//             // Parallel.ForEach(PubSub.GetInvalidatedShells(), shell =>
//             // {
//             //     // Capture previous state
//             //     // var isDebounced = shell.CapturePreviousState();

//             //     // // Capture current state
//             //     // if (shell.LastTick > 16.ms)
//             //     // {

//             //     // }

//             //     // Diff
//             // });

//             batch++;
//         }

//         Console.WriteLine("--- Goodbye from the UI Thread! ---");
//     }

//     private class UISynchronizationContext : SynchronizationContext
//     {
//         public override void Post(SendOrPostCallback d, object? state)
//         {
//             State.Invoke(() => d(state));
//         }

//         public override void Send(SendOrPostCallback d, object? state)
//         {
//             base.Send(d, state);
//         }

//         public override int Wait(nint[] waitHandles, bool waitAll, int millisecondsTimeout)
//         {
//             return base.Wait(waitHandles, waitAll, millisecondsTimeout);
//         }

//         public override void OperationCompleted()
//         {
//             Console.WriteLine("OperationCompleted()");
//             base.OperationCompleted();
//         }

//         public override void OperationStarted()
//         {
//             Console.WriteLine("OperationStarted()");
//             base.OperationStarted();
//         }

//         public override SynchronizationContext CreateCopy()
//         {
//             Console.WriteLine("CreateCopy()");
//             return base.CreateCopy();
//         }
//     }
// }