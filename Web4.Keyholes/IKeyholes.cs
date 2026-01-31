namespace Web4;

// TODO: It is awkward that this interface is here and not in the core project, thanks to Keyholes.
public interface IKeyholes
{
    void SetText(byte[] key, ref Keyhole keyhole);
    void SetAttribute(byte[] key, ref Keyhole keyhole);
    void SetAttribute(byte[] key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName);
    void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionName, ValueTuple<string, int> viewTransitionNameSecondary);
    void PushNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, byte[] newKey);
    void PushNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName);
    void PopNode(byte[] key);
    void PopNode(byte[] key, ValueTuple<string, int> viewTransitionName);
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Ping();
    void Dump();
    void Benchmark(int? threads);
}
