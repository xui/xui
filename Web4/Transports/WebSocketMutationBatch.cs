using Microsoft.Extensions.ObjectPool;

namespace Web4.Transports;

public struct WebSocketMutationBatch() : IMutationBatch
{
    private static readonly ObjectPool<JsonRpcWriter> pool = ObjectPool.Create<JsonRpcWriter>();
    private JsonRpcWriter? writer = null;
    public readonly ReadOnlyMemory<byte>? Buffer { get => writer?.Result; }

    public void UpdateValue(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setTextNode", ref newKeyhole);
    }

    public void UpdateAttribute(string key, ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setAttribute", ref newKeyhole);
    }

    public void UpdateAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setAttribute", key, newKeyholes, includeSentinels: false);
    }

    public void UpdatePartial(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setElement", key, newKeyholes, includeSentinels: true);
    }

    public void AddPartial(string key, int index, Span<Keyhole> keyholes)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("addElement", key, keyholes);
    }

    public void RemovePartial(string key, int index, Span<Keyhole> keyholes)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("removeElement", key, keyholes);
    }

    public void MovePartial(string key, int from, int to)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("moveElement", key, keyholes);
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