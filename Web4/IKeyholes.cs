namespace Web4;

// TODO: It is awkward that this interface is here and not in the core project, thanks to Keyholes.
public interface IKeyholes
{
    void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole);

    void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole);

    void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes);

    void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null);

    void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition = null);

    void RemoveElement(string key, string? transition = null);
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Dump();
}
