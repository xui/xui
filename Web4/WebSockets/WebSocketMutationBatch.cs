namespace Web4.WebSockets;

public struct WebSocketMutationBatch() : IMutationBatch
{
    private JsonRpcWriter? writer = null;
    public readonly ReadOnlyMemory<byte>? Buffer { get => writer?.Result; }

    public void SetTextNode(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= JsonRpcWriter.Pool.Get().BeginBatch();
        writer.WriteRpc("app.keyholes.setTextNode", ref newKeyhole);
    }

    public void SetAttribute(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= JsonRpcWriter.Pool.Get().BeginBatch();
        writer.WriteRpc("app.keyholes.setAttribute", ref newKeyhole);
    }

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        writer ??= JsonRpcWriter.Pool.Get().BeginBatch();
        writer.WriteRpc("app.keyholes.setAttribute", key, newKeyholes, includeSentinels: false);
    }

    public void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null)
    {
        writer ??= JsonRpcWriter.Pool.Get().BeginBatch();
        writer.WriteRpc("app.keyholes.setElement", key, newKeyholes, includeSentinels: true, transition);
    }

    public void AddElement(string key, string priorKey, Span<Keyhole> keyholes, string? transition = null)
    {
        writer ??= JsonRpcWriter.Pool.Get().BeginBatch();
        writer.WriteRpc("app.keyholes.addElement", key, priorKey, keyholes, includeSentinels: true, transition);
    }

    public void RemoveElement(string key, string? transition = null)
    {
        writer ??= JsonRpcWriter.Pool.Get().BeginBatch();
        writer.WriteRpc("app.keyholes.removeElement", key, transition);
    }

    public void Commit()
    {
        writer?.EndBatch();
    }

    public void Dispose()
    {
        if (writer is not null)
            JsonRpcWriter.Pool.Return(writer);
    }
}