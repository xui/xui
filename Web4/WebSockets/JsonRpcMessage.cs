using System.Buffers;

namespace Web4.WebSockets;

struct JsonRpcMessage(ReadOnlySequence<byte> sequence, string method, int? id)
{
    public string Method => method;
    public int? ID => id;
    public JsonRpcReader GetParams() => new(sequence);
}
