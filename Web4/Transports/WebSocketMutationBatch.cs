using System.Net.WebSockets;


namespace Web4.Transports;

public ref struct WebSocketMutationBatch() : IMutationBatch
{
    private JsonRpcWriter jsonRpcWriter = new();
    public readonly JsonRpcWriter Writer { get => jsonRpcWriter; }

    public void UpdateValue(string key, ref Keyhole before, ref Keyhole after)
    {
        ref var writer = ref jsonRpcWriter;
        writer.WriteRpc("mutate", ref after);
    }

    public void UpdateAttribute(string key, Span<Keyhole> before, Span<Keyhole> after)
    {
        ref var writer = ref jsonRpcWriter;
        writer.WriteRpc("mutate", key, after);
    }

    public void UpdatePartial(string key, Span<Keyhole> before, Span<Keyhole> after)
    {
    }

    public void AddPartial(string key, int index, Span<Keyhole> partial)
    {
    }

    public void RemovePartial(string key, int index, Span<Keyhole> partial)
    {
    }

    public void MovePartial(string key, int from, int to)
    {
    }

    public void Commit()
    {
        ref var writer = ref jsonRpcWriter;
        writer.EndBatch();
    }
}