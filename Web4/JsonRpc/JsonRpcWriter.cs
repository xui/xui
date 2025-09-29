using System.Buffers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Web4.JsonRpc;

public class JsonRpcWriter: IDisposable, IResettable
{
    private readonly CopyToGrowBufferWriter bufferWriter;
    private readonly Utf8JsonWriter jsonWriter;

    public ReadOnlyMemory<byte> AsMemory() => bufferWriter.AsMemory();

    public JsonRpcWriter(int bufferSize = 1024)
    {
        bufferWriter = new(bufferSize);
        jsonWriter = new(bufferWriter);
    }

    public JsonRpcWriter BeginBatch()
    {
        jsonWriter.WriteStartArray();
        return this;
    }

    public JsonRpcWriter EndBatch()
    {
        jsonWriter.WriteEndArray();
        jsonWriter.Flush();
        return this;
    }

    public void WriteNotification(string method)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification(ValueTuple<string, string, string> method)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification<T>(string method, T param)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteTValue(param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification<T>(ValueTuple<string, string, string> method, T param)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteTValue(param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification(string method, string value1, params Span<string> @values)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        jsonWriter.WriteStringValue(value1);
        foreach (var value in values)
            jsonWriter.WriteStringValue(value);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification(string method, params Span<object> @values)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        foreach (var value in values)
            jsonWriter.WriteStringValue(value.ToString());
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification(ValueTuple<string, string, string> method, ref Keyhole param)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteKeyholeValue(ref param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();
    }

    public void WriteNotification(ValueTuple<string, string, string> method, Span<Keyhole> keyholes, bool includeSentinels, string? transition = null)
        => WriteNotification(method, null, keyholes, includeSentinels, transition);
        
    public void WriteNotification(ValueTuple<string, string, string> method, string? param1, Span<Keyhole> param2, bool includeSentinels, string? transition = null)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");

        if (param1 is not null)
            jsonWriter.WriteStringValue(param1);

        Span<Keyhole> keyholes = param2; // TODO: Start here
        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            if (keyhole.Type == KeyholeType.StringLiteral)
            {
                var isLast = i == keyholes.Length - 1;
                jsonWriter.WriteStringValueSegment(keyhole.StringLiteral, isLast);
                continue;
            }

            if (includeSentinels)
            {
                jsonWriter.WriteStringValueSegment("<!-- -->", false);
            }

            switch (keyhole.Type)
            {
                case KeyholeType.String:
                    jsonWriter.WriteStringValueSegment(keyhole.String, false);
                    break;
                case KeyholeType.Boolean:
                    jsonWriter.WriteStringValueSegment(keyhole.Boolean ? "true" : "false", false);
                    break;
                default:
                    jsonWriter.Flush();
                    WriteKeyholeToRawBuffer(ref keyhole);
                    break;
            }

            if (includeSentinels)
            {
                jsonWriter.WriteStringValueSegment("<!--", false);
                jsonWriter.WriteStringValueSegment(keyhole.Key, false);
                jsonWriter.WriteStringValueSegment("-->", false);
            }
        }

        if (transition is not null)
        {
            jsonWriter.WriteStringValue(transition);
        }

        jsonWriter.WriteEndArray();
        
        jsonWriter.WriteEndObject();
    }

    public void WriteRequest(int id)
    {
        // TODO: Implement
    }

    public void WriteResponse(int id)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");
        jsonWriter.WriteNull("result");
        jsonWriter.WriteNumber("id", id);
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
    }

    public void WriteResponse<T>(int id, T result)
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");
        jsonWriter.WritePropertyName("result");
        WriteTValue(result);
        jsonWriter.WriteNumber("id", id);
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();
    }

    private void WriteMethod(string method)
    {
        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValue(method);
    }

    private void WriteMethod(ValueTuple<string, string, string> method)
    {
        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(".", false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(".", false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);
    }

    private void WriteTValue<T>(T value)
    {
        switch (value)
        {
            case string s:
                jsonWriter.WriteStringValue(s);
                break;
            case int i:
                jsonWriter.WriteNumberValue(i);
                break;
            case bool b:
                jsonWriter.WriteBooleanValue(b);
                break;
            // TODO: Support the rest.
            default:
                jsonWriter.WriteNullValue();
                break;
        }
    }

    private void WriteKeyholeValue(ref Keyhole keyhole)
    {
        // String and Boolean do not use format strings.
        switch (keyhole.Type)
        {
            case KeyholeType.String:
                jsonWriter.WriteStringValue(keyhole.String);
                return;
            case KeyholeType.Boolean:
                jsonWriter.WriteBooleanValue(keyhole.Boolean);
                return;
        }

        jsonWriter.Flush();
        Encoding.UTF8.GetBytes("\"", bufferWriter);
        WriteKeyholeToRawBuffer(ref keyhole);
        Encoding.UTF8.GetBytes("\"", bufferWriter);
    }

    private void WriteKeyholeToRawBuffer(ref Keyhole keyhole)
    {
        // TODO: Does Writer.GetSpan() need a length?  What's the max length of all T's?
        int length = 0;
        switch (keyhole.Type)
        {
            case KeyholeType.Integer:
                while (!keyhole.Integer.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.Long:
                while (!keyhole.Long.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.Float:
                while (!keyhole.Float.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.Double:
                while (!keyhole.Double.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.Decimal:
                while (!keyhole.Decimal.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.DateTime:
                while (!keyhole.DateTime.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.DateOnly:
                while (!keyhole.DateOnly.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.TimeSpan:
                while (!keyhole.TimeSpan.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.TimeOnly:
                while (!keyhole.TimeOnly.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.Color:
                while (!keyhole.Color.TryFormat(bufferWriter.GetSpan(9), out length, keyhole.Format))
                    bufferWriter.GrowBuffer();
                break;
            case KeyholeType.Uri:
                // TODO: Fix memory allocation and support format string?
                Encoding.UTF8.GetBytes(keyhole.Uri!.ToString(), bufferWriter);
                break;
        }
        bufferWriter.Advance(length);
    }

    public bool TryReset()
    {
        bufferWriter.TryReset();
        jsonWriter.Reset(bufferWriter);
        return true;
    }

    public void Dispose()
    {
        bufferWriter.Dispose();
        jsonWriter.Dispose();
    }

    private class CopyToGrowBufferWriter(int bufferSize) : IBufferWriter<byte>, IResettable, IDisposable
    {
        private byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        private int cursor = 0;

        public ReadOnlyMemory<byte> AsMemory() => buffer.AsMemory(..cursor);

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

        public void GrowBuffer(int sizeHint = 1)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            // Growing by small increments is wasteful.  Buffer-growth should at least double.
            // Skip the gradual doubling if we know it won't be enough.
            int newLength = Math.Max(sizeHint, buffer.Length) + buffer.Length;

            Console.WriteLine($"⚠️ Had to grow buffer from {buffer.Length} to {newLength}");

            // TODO: Should this be a configuration somewhere?
            if (newLength > 100_000_000)
                throw new ApplicationException("Max buffer size exceeded.");

            var oldBuffer = buffer;
            var newBuffer = ArrayPool<byte>.Shared.Rent(newLength);
            oldBuffer.CopyTo(newBuffer, 0);
            buffer = newBuffer;
            ArrayPool<byte>.Shared.Return(oldBuffer);
        }

        public bool TryReset()
        {
            cursor = 0;
            return true;
        }

        public void Dispose()
        {
            if (buffer is not null)
                ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}