using System.Buffers;
using System.Text.Json;

namespace Web4.WebSockets;

record struct JsonRpc(string Method, int? ID)
{
    public static JsonRpc Error = new("error", null);

    public T GetNextPositionalParam<T>()
    {
        return default(T);
    }
}
