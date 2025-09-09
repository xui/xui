using System.Buffers;
using System.Text.Json;

namespace Web4.WebSockets;

record struct JsonRpcMessage(string Method, int? ID);
