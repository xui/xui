namespace Web4.WebSockets;

public struct WebSocketMutationBatch() : IMutationBatch
{
    private JsonRpcWriter? writer = null;
    public readonly ReadOnlyMemory<byte>? Buffer { get => writer?.AsMemory(); }

    public void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification("setTextNode", ref newKeyhole);
    }

    public void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification("setAttribute", ref newKeyhole);
    }

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification("setAttribute", key, newKeyholes, includeSentinels: false);
    }

    public void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification("setElement", key, newKeyholes, includeSentinels: true, transition);
    }

    public void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition = null)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification("addElement", priorKey, key, keyholes, includeSentinels: true, transition);
    }

    public void RemoveElement(string key, string? transition = null)
    {
        writer ??= new JsonRpcWriter().BeginBatch();
        writer?.WriteNotification("removeElement", key, transition);
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