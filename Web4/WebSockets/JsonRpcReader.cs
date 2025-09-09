using System.Buffers;
using System.Text.Json;

namespace Web4.WebSockets;

ref struct JsonRpcReader(ReadOnlySequence<byte> sequence)
{
    Utf8JsonReader reader = new(sequence);

    public static JsonRpcMessage ParseMessage(ReadOnlySequence<byte> sequence)
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

        return new(method ?? "error", id);
    }

    public static JsonRpcReader LazyParseParams(ReadOnlySequence<byte> sequence) => new(sequence);

    private void ParseToParams()
    {
        if (reader.TokenStartIndex > 0)
            return;
        
        using var perf = Debug.PerfCheck("JsonRpcReader.ParseToParams"); // TODO: Remove PerfCheck

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("params"))
                {
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        return;
                    }
                }
                else
                {
                    reader.Skip();
                }
            }
        }
    }

    public int GetNextInt()
    {
        ParseToParams();
        reader.Read();
        return reader.GetInt32();
    }

    public int? GetNextNullableInt()
    {
        ParseToParams();
        reader.Read();
        if (reader.TryGetInt32(out int value))
            return value;
        return null;
    }

    public string GetNextString()
    {
        ParseToParams();
        reader.Read();
        return reader.GetString()!;
    }

    public LazyEvent GetNextEvent()
    {
        ParseToParams();
        reader.Read();
        reader.Skip();
        return new(sequence);
    }
}