using System.Buffers;
using System.Text.Json;

namespace Web4.WebSockets;

record struct JsonRpc(string Method, int? ID);
