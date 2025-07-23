using Microsoft.Extensions.ObjectPool;

namespace Web4.Transports;

public struct WebSocketMutationBatch() : IMutationBatch
{
    private static readonly ObjectPool<JsonRpcWriter> pool = ObjectPool.Create<JsonRpcWriter>();
    private JsonRpcWriter? writer = null;
    public readonly ReadOnlyMemory<byte>? Buffer { get => writer?.Result; }

    public void UpdateValue(string key, ref Keyhole before, ref Keyhole after)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setTextNode", ref after);
    }

    public void UpdateAttribute(string key, Span<Keyhole> before, Span<Keyhole> after)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setAttribute", key, after, includeSentinels: false);
    }

    public void UpdatePartial(string key, Span<Keyhole> before, Span<Keyhole> after)
    {
        writer ??= pool.Get().BeginBatch();
        writer.WriteRpc("setElement", key, after, includeSentinels: true);
    }

    public void AddPartial(string key, int index, Span<Keyhole> partial)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("addElement", key, after);
    }

    public void RemovePartial(string key, int index, Span<Keyhole> partial)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("removeElement", key, after);
    }

    public void MovePartial(string key, int from, int to)
    {
        writer ??= pool.Get().BeginBatch();
        // writer.WriteRpc("moveElement", key, after);
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