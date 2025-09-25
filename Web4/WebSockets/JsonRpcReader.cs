using System.Buffers;
using System.Text.Json;
using Web4.Core.DOM;

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

        return new(sequence, method ?? "error", id);
    }

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
        using var perf = Debug.PerfCheck("JsonRpcReader.GetNextInt"); // TODO: Remove PerfCheck        
        reader.Read();
        return reader.GetInt32();
    }

    public int? GetNextNullableInt()
    {
        ParseToParams();
        using var perf = Debug.PerfCheck("JsonRpcReader.GetNextNullableInt"); // TODO: Remove PerfCheck        
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray && reader.TryGetInt32(out int value))
            return value;
        return null;
    }

    public string GetNextString()
    {
        ParseToParams();
        using var perf = Debug.PerfCheck("JsonRpcReader.GetNextString"); // TODO: Remove PerfCheck        
        reader.Read();
        return reader.GetString()!;
    }

    public string? GetNextKey()
    {
        ParseToParams();
        using var perf = Debug.PerfCheck("JsonRpcReader.GetNextKey"); // TODO: Remove PerfCheck        
        reader.Read();
        return reader.HasValueSequence
            ? Keymaker.GetKeyIfCached(reader.ValueSequence)
            : Keymaker.GetKeyIfCached(reader.ValueSpan);
    }

    public LazyEvent GetNextEvent(WebSocketTransport transport)
    {
        ParseToParams();
        using var perf = Debug.PerfCheck("JsonRpcReader.GetNextEvent"); // TODO: Remove PerfCheck        
        reader.Read();
        reader.Skip();
        return new(sequence, transport);
    }
}