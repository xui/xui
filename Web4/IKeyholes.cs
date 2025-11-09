namespace Web4;

// TODO: It is awkward that this interface is here and not in the core project, thanks to Keyholes.
public interface IKeyholes
{
    void SetText(string key, ref Keyhole keyhole);
    void SetAttribute(string key, ref Keyhole keyhole);
    void SetAttribute(string key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] buffer, string key, Span<Keyhole> keyholes);
    void SetNode(Keyhole[] buffer, string key, Span<Keyhole> keyholes, ValueTuple<string, string> viewTransitionName);
    void SetNode(Keyhole[] buffer, string key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionName, ValueTuple<string, int> viewTransitionNameSecondary);
    void PushNode(Keyhole[] buffer, string key, Span<Keyhole> keyholes, string newKey);
    void PushNode(Keyhole[] buffer, string key, Span<Keyhole> keyholes, string newKey, ValueTuple<string, int> viewTransitionName);
    void PopNode(string key);
    void PopNode(string key, ValueTuple<string, int> viewTransitionName);
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Ping();
    void Dump();
    void Benchmark(int? threads);
}
