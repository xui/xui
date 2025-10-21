namespace Web4;

// TODO: It is awkward that this interface is here and not in the core project, thanks to Keyholes.
public interface IKeyholes
{
    void SetTextNode(string key, ref Keyhole keyhole);
    void SetAttribute(string key, ref Keyhole keyhole);
    void SetAttribute(string key, Span<Keyhole> keyholes);
    void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes);
    void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, bool reverseTransition);
    void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, object oldTag, object newTag);
    void AddElement(Keyhole[] buffer, string priorKey, string key, Span<Keyhole> keyholes, string? transition = null);
    void RemoveElement(string key, string? transition = null);
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Dump();
}
