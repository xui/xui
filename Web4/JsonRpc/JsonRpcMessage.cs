using System.Buffers;
using System.Text.Json;

namespace Web4.JsonRpc;

public readonly ref struct JsonRpcMessage
{
    public ReadOnlySpan<byte> Version { get; init; }
    public ReadOnlySpan<byte> Method { get; init; }
    public ReadOnlySpan<byte> Id { get; init; }
    public int? IdAsInt => int.TryParse(Id, out int result) ? result : null;
    private readonly ReadOnlySequence<byte> paramsSequence;
    public PositionalParams GetPositionalParams() => new PositionalParams(paramsSequence);
    public ReadOnlySequence<byte> Result { get; init; }
    public ReadOnlySequence<byte> Error { get; init; }

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
                    paramsSequence = ReadSequence(sequence, ref reader);
                }
                else if (reader.ValueTextEquals("result"u8))
                {
                    reader.Read();
                    Result = ReadSequence(sequence, ref reader);
                }
                else if (reader.ValueTextEquals("error"u8))
                {
                    reader.Read();
                    Error = ReadSequence(sequence, ref reader);
                }
            }
        }
    }

    internal static ReadOnlySequence<byte> ReadSequence(ReadOnlySequence<byte> sequence, ref Utf8JsonReader reader)
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

        public int? GetNextOptionalAsInt()
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

        public ReadOnlySpan<byte> GetNextAsSpan()
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsSpan"); // TODO: Remove PerfCheck        
            reader.Read();
            return reader.SafeValueSpan;
        }

        public ReadOnlySequence<byte> GetNextAsSequence()
        {
            using var perf = Debug.PerfCheck("JsonRpcMessage.GetNextAsSequence"); // TODO: Remove PerfCheck        
            reader.Read();
            return JsonRpcMessage.ReadSequence(sequence, ref reader);
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