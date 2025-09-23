namespace Web4.WebSockets;

public struct WebSocketMutationBatch() : IMutationBatch
{
    private JsonRpcWriter? writer = null;
    public readonly ReadOnlyMemory<byte>? Buffer { get => writer?.AsMemory(); }

    public void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setTextNode"),
            param: ref newKeyhole
        );
    }

    public void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setAttribute"),
            param: ref newKeyhole
        );
    }

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification(
            method: ("app.keyholes", key, "setAttribute"),
            newKeyholes,
            includeSentinels: false
        );
    }

    public void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification(
            method: ("app.keyholes", key, "setElement"),
            newKeyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    public void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition = null)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification(
            method: ("app.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    public void RemoveElement(string key, string? transition = null)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        if (transition is null)
            writer?.WriteNotification(
                method: ("app.keyholes", key, "removeElement")
            );
        else
            writer?.WriteNotification(
                method: ("app.keyholes", key, "removeElement"),
                param: transition
            );
    }

    public void Commit()
    {
        writer?.EndBatch();
    }

    public void Dispose()
    {
        writer?.Dispose();
        writer = null;
    }
}