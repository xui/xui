namespace Web4;

// TODO: It is awkward that this interface is here and not in the core project, thanks to Keyholes.
public interface IKeyholes
{
    void SetTextNode(string key, ref Keyhole keyhole);
    void SetAttribute(string key, ref Keyhole keyhole);
    void SetAttribute(string key, Span<Keyhole> keyholes);
    void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes);
    void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, ValueTuple<string, string> viewTransitionName);
    void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionName, ValueTuple<string, int> viewTransitionNameSecondary);
    void AddElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, string newKey);
    void AddElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, string newKey, ValueTuple<string, int> viewTransitionName);
    void RemoveElement(string key);
    void RemoveElement(string key, ValueTuple<string, int> viewTransitionName);
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Ping();
    void Dump();
    void Benchmark(int? threads);
}
