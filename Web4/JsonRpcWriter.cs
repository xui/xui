using System.Buffers;
using System.Drawing;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Web4;

public class JsonRpcWriter : IBufferWriter<byte>, IResettable, IDisposable
{
    public static ObjectPool<JsonRpcWriter> Pool { get; } = ObjectPool.Create<JsonRpcWriter>();

    private int cursor = 0;
    private byte[] buffer;
    private readonly Utf8JsonWriter utf8JsonWriter;

    public JsonRpcWriter() : this(1024) { }

    public JsonRpcWriter(int bufferSize)
    {
        buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        utf8JsonWriter = new(this);
    }

    public ReadOnlyMemory<byte>? Result => buffer?.AsMemory(..cursor);

    public JsonRpcWriter BeginBatch()
    {
        utf8JsonWriter.WriteStartArray();
        return this;
    }

    public JsonRpcWriter EndBatch()
    {
        utf8JsonWriter.WriteEndArray();
        utf8JsonWriter.Flush();
        return this;
    }

    public void WriteRpc(string method, ref Keyhole keyhole)
    {
        utf8JsonWriter.WriteStartObject();
        utf8JsonWriter.WriteString("jsonrpc", "2.0");
        utf8JsonWriter.WriteString("method", method);
        utf8JsonWriter.WriteStartArray("params");
        utf8JsonWriter.WriteStringValue(keyhole.Key);
        WriteKeyholeValue(ref keyhole);
        utf8JsonWriter.WriteEndArray();
        utf8JsonWriter.WriteEndObject();
    }

    public void WriteRpc(string method, string key, string? transition = null)
    {
        utf8JsonWriter.WriteStartObject();
        utf8JsonWriter.WriteString("jsonrpc", "2.0");
        utf8JsonWriter.WriteString("method", method);
        utf8JsonWriter.WriteStartArray("params");
        utf8JsonWriter.WriteStringValue(key);
        if (transition != null)
            utf8JsonWriter.WriteStringValue(transition);
        utf8JsonWriter.WriteEndArray();
        utf8JsonWriter.WriteEndObject();
    }

    public void WriteRpc(string method, string key, Span<Keyhole> keyholes, bool includeSentinels, string? transition = null)
    {
        utf8JsonWriter.WriteStartObject();
        utf8JsonWriter.WriteString("jsonrpc", "2.0");
        utf8JsonWriter.WriteString("method", method);
        utf8JsonWriter.WriteStartArray("params");

        utf8JsonWriter.WriteStringValue(key);

        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            if (keyhole.Type == KeyholeType.StringLiteral)
            {
                var isLast = i == keyholes.Length - 1;
                utf8JsonWriter.WriteStringValueSegment(keyhole.StringLiteral, isLast);
                continue;
            }

            if (includeSentinels)
            {
                utf8JsonWriter.WriteStringValueSegment("<!-- -->", false);
            }

            switch (keyhole.Type)
            {
                case KeyholeType.String:
                    utf8JsonWriter.WriteStringValueSegment(keyhole.String, false);
                    break;
                case KeyholeType.Boolean:
                    utf8JsonWriter.WriteStringValueSegment(keyhole.Boolean ? "true" : "false", false);
                    break;
                default:
                    utf8JsonWriter.Flush();
                    WriteRawWithFormatString(ref keyhole);
                    break;
            }

            if (includeSentinels)
            {
                utf8JsonWriter.WriteStringValueSegment("<!--", false);
                utf8JsonWriter.WriteStringValueSegment(keyhole.Key, false);
                utf8JsonWriter.WriteStringValueSegment("-->", false);
            }
        }

        if (transition is not null)
        {
            utf8JsonWriter.WriteStringValue(transition);
        }

        utf8JsonWriter.WriteEndArray();
        utf8JsonWriter.WriteEndObject();
    }

    public void WriteResult(int id)
    {
        utf8JsonWriter.WriteStartObject();
        utf8JsonWriter.WriteString("jsonrpc", "2.0");
        utf8JsonWriter.WriteNull("result");
        utf8JsonWriter.WriteNumber("id", id);
        utf8JsonWriter.WriteEndObject();
        utf8JsonWriter.Flush();
    }

    public void WriteResult(int id, string? result)
    {
        utf8JsonWriter.WriteStartObject();
        utf8JsonWriter.WriteString("jsonrpc", "2.0");
        if (result is not null)
            utf8JsonWriter.WriteString("result", result);
        else
            utf8JsonWriter.WriteNull("result");
        utf8JsonWriter.WriteNumber("id", id);
        utf8JsonWriter.WriteEndObject();
        utf8JsonWriter.Flush();
    }

    private void WriteKeyholeValue(ref Keyhole keyhole)
    {
        // String and Boolean do not use format strings.
        switch (keyhole.Type)
        {
            case KeyholeType.String:
                utf8JsonWriter.WriteStringValue(keyhole.String);
                return;
            case KeyholeType.Boolean:
                utf8JsonWriter.WriteBooleanValue(keyhole.Boolean);
                return;
        }

        utf8JsonWriter.Flush();
        Encoding.UTF8.GetBytes(",\"", this);
        WriteRawWithFormatString(ref keyhole);
        Encoding.UTF8.GetBytes("\"", this);
    }

    private void WriteRawWithFormatString(ref Keyhole keyhole)
    {
        // TODO: Does Writer.GetSpan() need a length?  What's the max length of all T's?
        int length = 0;
        switch (keyhole.Type)
        {
            case KeyholeType.Color:
                while (!keyhole.Color.TryFormat(GetSpan(9), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.Uri:
                // TODO: Fix memory allocation and support format string?
                Encoding.UTF8.GetBytes(keyhole.Uri!.ToString(), this);
                break;
            case KeyholeType.Integer:
                while (!keyhole.Integer.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.Long:
                while (!keyhole.Long.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.Float:
                while (!keyhole.Float.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.Double:
                while (!keyhole.Double.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.Decimal:
                while (!keyhole.Decimal.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.DateTime:
                while (!keyhole.DateTime.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.DateOnly:
                while (!keyhole.DateOnly.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.TimeSpan:
                while (!keyhole.TimeSpan.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
            case KeyholeType.TimeOnly:
                while (!keyhole.TimeOnly.TryFormat(GetSpan(), out length, keyhole.Format))
                    GrowBuffer();
                break;
        }
        Advance(length);
    }

    private void GrowBuffer(int sizeHint = 1)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        // Growing by small increments is wasteful.  Buffer-growth should at least double.
        // Skip the gradual doubling if we know it won't be enough.
        int newLength = Math.Max(sizeHint, buffer.Length) + buffer.Length;

        // TODO: Should this be a configuration somewhere?
        if (newLength > 100_000_000)
            throw new ApplicationException("Max buffer size exceeded.");

        var oldBuffer = buffer;
        var newBuffer = ArrayPool<byte>.Shared.Rent(newLength);
        oldBuffer.CopyTo(newBuffer, 0);
        buffer = newBuffer;
        ArrayPool<byte>.Shared.Return(oldBuffer);
    }

    public void Advance(int count)
    {
        cursor += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (cursor + sizeHint > buffer.Length)
            GrowBuffer();
        return buffer.AsMemory(cursor..);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (cursor + sizeHint > buffer.Length)
            GrowBuffer();
        return buffer.AsSpan(cursor..);
    }

    public bool TryReset()
    {
        cursor = 0;
        utf8JsonWriter.Reset(this);
        return true;
    }

    public void Dispose()
    {
        if (buffer is not null)
            ArrayPool<byte>.Shared.Return(buffer);
    }
}