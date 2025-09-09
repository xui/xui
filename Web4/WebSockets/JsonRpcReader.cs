using System.Buffers;
using System.Text.Json;

namespace Web4.WebSockets;

ref struct JsonRpcReader(ReadOnlySequence<byte> sequence)
{
    Utf8JsonReader reader = new(sequence);

    public static JsonRpc ParseMessage(ReadOnlySequence<byte> sequence)
    {
        using var perf = Debug.PerfCheck("JsonRpcReader.Parse"); // TODO: Remove PerfCheck

        var rpc = new JsonRpcReader(sequence);
        string? method = null;
        int? id = null;

        while (rpc.reader.Read())
        {
            if (rpc.reader.TokenType == JsonTokenType.PropertyName)
            {
                if (rpc.reader.ValueTextEquals("method"))
                {
                    rpc.reader.Read();
                    ReadOnlySpan<byte> value = rpc.reader.HasValueSequence
                        ? rpc.reader.ValueSequence.ToArray() // TODO: Allocates, but is rare?  Confirm.
                        : rpc.reader.ValueSpan;
                    method = Keymaker.GetKeyIfCached(value);
                }
                else if (rpc.reader.ValueTextEquals("params"))
                {
                    rpc.reader.Skip();
                }
                else if (rpc.reader.ValueTextEquals("id"))
                {
                    rpc.reader.Read();
                    if (rpc.reader.TryGetInt32(out int i))
                        id = i;
                }
            }
        }

        return new(sequence) { Method = method, ID = id };
    }

    public static JsonRpcReader ParseParams(ReadOnlySequence<byte> sequence)
    {
        using var perf = Debug.PerfCheck("JsonRpcReader.ParseParams"); // TODO: Remove PerfCheck

        var rpc = new JsonRpcReader(sequence);

        while (rpc.reader.Read())
        {
            if (rpc.reader.TokenType == JsonTokenType.PropertyName)
            {
                if (rpc.reader.ValueTextEquals("params"))
                {
                    rpc.reader.Read();
                    if (rpc.reader.TokenType == JsonTokenType.StartArray)
                    {
                        return rpc;
                    }
                }
                else
                {
                    rpc.reader.Skip();
                }
            }
        }

        return rpc;
    }

    public int GetNextInt()
    {
        reader.Read();
        return reader.GetInt32();
    }

    public string GetNextString()
    {
        reader.Read();
        return reader.GetString()!;
    }

    public LazyEvent GetNextEvent()
    {
        reader.Read();
        reader.Skip();
        return new(sequence);
    }
}