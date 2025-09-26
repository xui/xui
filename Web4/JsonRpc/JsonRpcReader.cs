using System.Buffers;
using System.Text.Json;
using Web4.WebSockets;

namespace Web4.JsonRpc;

public readonly ref struct JsonRpcMessage
{
    public ReadOnlySpan<byte> Version { get; init; } = default;
    public ReadOnlySpan<byte> Method { get; init; } = default;
    public ReadOnlySpan<byte> Id { get; init; } = default;
    public int? IdAsInt => int.TryParse(Id, out int result) ? result : null;
    private ReadOnlySequence<byte> Params { get; init; } = default;
    public PositionalParams GetPositionalParams() => new PositionalParams(Params);
    public ReadOnlySequence<byte> Result { get; init; } = default;
    public ReadOnlySequence<byte> Error { get; init; } = default;

    public JsonRpcMessage(ReadOnlySequence<byte> sequence)
    {
        using var perf = Debug.PerfCheck("JsonRpcReader.Parse"); // TODO: Remove PerfCheck
        var reader = new Utf8JsonReader(sequence);
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("jsonrpc"u8))
                {
                    reader.Read();
                    Version = reader.SafeValueSpan;
                }
                else if (reader.ValueTextEquals("method"u8))
                {
                    reader.Read();
                    Method = reader.SafeValueSpan;
                }
                else if (reader.ValueTextEquals("id"u8))
                {
                    reader.Read();
                    Id = reader.SafeValueSpan;
                }
                else if (reader.ValueTextEquals("params"u8))
                {
                    reader.Read();
                    Params = ReadSequence(sequence, ref reader);
                }
                else if (reader.ValueTextEquals("result"u8))
                {
                    Result = ReadSequence(sequence, ref reader);
                }
                else if (reader.ValueTextEquals("error"u8))
                {
                    Error = ReadSequence(sequence, ref reader);
                }
            }
        }
    }

    private static ReadOnlySequence<byte> ReadSequence(ReadOnlySequence<byte> sequence, ref Utf8JsonReader reader)
    {
        var start = reader.TokenStartIndex;
        reader.Skip();
        var end = reader.BytesConsumed;
        return sequence.Slice(start, end - start);
    }

    public ref struct PositionalParams
    {
        private ReadOnlySequence<byte> sequence;
        private Utf8JsonReader reader;

        public PositionalParams(ReadOnlySequence<byte> sequence)
        {
            this.sequence = sequence;
            if (sequence.Length > 0)
            {
                reader = new Utf8JsonReader(sequence);
                reader.Read(); // reader.TokenType == JsonTokenType.StartArray
            }
        }

        public int GetNextAsInt()
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsInt"); // TODO: Remove PerfCheck        
            reader.Read();
            return reader.GetInt32();
        }

        // Nullable since it might not be included at the end.
        public int? GetNextAsNullableInt()
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsNullableInt"); // TODO: Remove PerfCheck        
            reader.Read();
            return reader.TokenType != JsonTokenType.EndArray && reader.TryGetInt32(out int value)
                ? value
                : null;
        }

        public string GetNextAsString()
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsString"); // TODO: Remove PerfCheck        
            reader.Read();
            return reader.GetString()!;
        }

        public string? GetNextAsKey()
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsKey"); // TODO: Remove PerfCheck        
            reader.Read();
            return reader.HasValueSequence
                ? Keymaker.GetKeyIfCached(reader.ValueSequence)
                : Keymaker.GetKeyIfCached(reader.ValueSpan);
        }

        public LazyEvent GetNextAsEvent(WebSocketTransport transport)
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsEvent"); // TODO: Remove PerfCheck        
            reader.Read();
            reader.Skip();
            return new(sequence, transport);
        }
    }
}

public static class Utf8JsonReaderExtensions
{
    extension(Utf8JsonReader reader)
    {
        public ReadOnlySpan<byte> SafeValueSpan => reader.HasValueSequence
            ? reader.ValueSequence.ToArray() // TODO: Allocates, but is rare?  Confirm.
            : reader.ValueSpan;
    }
}