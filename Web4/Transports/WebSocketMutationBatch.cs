using Microsoft.Extensions.ObjectPool;

namespace Web4.Transports;

public struct WebSocketMutationBatch() : IMutationBatch
{
    private static readonly ObjectPool<JsonRpcWriter> pool = ObjectPool.Create<JsonRpcWriter>();
    private JsonRpcWriter? writer = null;
    public readonly ReadOnlyMemory<byte>? Buffer { get => writer?.Result; }

    public void SetTextNode(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("window.rpc.client.setTextNode", ref newKeyhole);
    }

    public void SetAttribute(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("window.rpc.client.setAttribute", ref newKeyhole);
    }

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("window.rpc.client.setAttribute", key, newKeyholes, includeSentinels: false);
    }

    public void ReplaceElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition = null)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("window.rpc.client.setElement", key, newKeyholes, includeSentinels: true, transition);
    }

    public void AddElement(string key, int index, Span<Keyhole> keyholes)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("window.rpc.client.addElement", key, keyholes);
    }

    public void RemoveElement(string key, int index, Span<Keyhole> keyholes)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("window.rpc.client.removeElement", key, keyholes);
    }

    public void MoveElement(string key, int from, int to)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("window.rpc.client.moveElement", key, keyholes);
    }

    public void Commit()
    {
        writer?.EndBatch();
    }

    public void Dispose()
    {
        if (writer is not null)
            pool.Return(writer);
    }
}