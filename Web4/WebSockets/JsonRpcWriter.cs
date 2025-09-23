using System.Buffers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;

namespace Web4.WebSockets;

public struct JsonRpcWriter(): IDisposable, IResettable
{
    private static readonly ObjectPool<WritersHolder> pool = ObjectPool.Create<WritersHolder>();

    private readonly WritersHolder writers = pool.Get();
    private CopyToGrowBufferWriter BufferWriter => writers.BufferWriter;
    private Utf8JsonWriter JsonWriter => writers.JsonWriter;

    public ReadOnlyMemory<byte> AsMemory() => writers.BufferWriter.AsMemory();

    public JsonRpcWriter BeginBatch()
    {
        JsonWriter.WriteStartArray();
        return this;
    }

    public JsonRpcWriter EndBatch()
    {
        JsonWriter.WriteEndArray();
        JsonWriter.Flush();
        return this;
    }

    public void WriteNotification(string method, string key, string? transition = null)
    {
        JsonWriter.WriteStartObject();

        JsonWriter.WriteString("jsonrpc", "2.0");

        JsonWriter.WritePropertyName("method");
        JsonWriter.WriteStringValueSegment("app.keyholes.", false);
        JsonWriter.WriteStringValueSegment(key, false);
        JsonWriter.WriteStringValueSegment(".", false);
        JsonWriter.WriteStringValueSegment(method, true);

        JsonWriter.WriteStartArray("params");
        JsonWriter.WriteStringValue(key);
        if (transition != null)
            JsonWriter.WriteStringValue(transition);
        JsonWriter.WriteEndArray();

        JsonWriter.WriteEndObject();
    }

    public void WriteNotification(string method, ref Keyhole keyhole)
    {
        JsonWriter.WriteStartObject();

        JsonWriter.WriteString("jsonrpc", "2.0");

        JsonWriter.WritePropertyName("method");
        JsonWriter.WriteStringValueSegment("app.keyholes.", false);
        JsonWriter.WriteStringValueSegment(keyhole.Key, false);
        JsonWriter.WriteStringValueSegment(".", false);
        JsonWriter.WriteStringValueSegment(method, true);

        JsonWriter.WriteStartArray("params");
        WriteKeyholeValue(ref keyhole);
        JsonWriter.WriteEndArray();

        JsonWriter.WriteEndObject();
    }

    public void WriteNotification(string method, string key, Span<Keyhole> keyholes, bool includeSentinels, string? transition = null)
        => WriteNotification(method, key, null, keyholes, includeSentinels, transition);
        
    public void WriteNotification(string method, string key1, string? key2, Span<Keyhole> keyholes, bool includeSentinels, string? transition = null)
    {
        JsonWriter.WriteStartObject();

        JsonWriter.WriteString("jsonrpc", "2.0");

        JsonWriter.WritePropertyName("method");
        JsonWriter.WriteStringValueSegment("app.keyholes.", false);
        JsonWriter.WriteStringValueSegment(key1, false);
        JsonWriter.WriteStringValueSegment(".", false);
        JsonWriter.WriteStringValueSegment(method, true);

        JsonWriter.WriteStartArray("params");

        if (key2 is not null)
            JsonWriter.WriteStringValue(key2);

        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            if (keyhole.Type == KeyholeType.StringLiteral)
            {
                var isLast = i == keyholes.Length - 1;
                JsonWriter.WriteStringValueSegment(keyhole.StringLiteral, isLast);
                continue;
            }

            if (includeSentinels)
            {
                JsonWriter.WriteStringValueSegment("<!-- -->", false);
            }

            switch (keyhole.Type)
            {
                case KeyholeType.String:
                    JsonWriter.WriteStringValueSegment(keyhole.String, false);
                    break;
                case KeyholeType.Boolean:
                    JsonWriter.WriteStringValueSegment(keyhole.Boolean ? "true" : "false", false);
                    break;
                default:
                    JsonWriter.Flush();
                    WriteKeyholeToRawBuffer(ref keyhole);
                    break;
            }

            if (includeSentinels)
            {
                JsonWriter.WriteStringValueSegment("<!--", false);
                JsonWriter.WriteStringValueSegment(keyhole.Key, false);
                JsonWriter.WriteStringValueSegment("-->", false);
            }
        }

        if (transition is not null)
        {
            JsonWriter.WriteStringValue(transition);
        }

        JsonWriter.WriteEndArray();
        
        JsonWriter.WriteEndObject();
    }

    public void WriteRequest(int id)
    {
        // TODO: Implement
    }

    public void WriteResponse(int id)
    {
        JsonWriter.WriteStartObject();
        JsonWriter.WriteString("jsonrpc", "2.0");
        JsonWriter.WriteNull("result");
        JsonWriter.WriteNumber("id", id);
        JsonWriter.WriteEndObject();
        JsonWriter.Flush();
    }

    public void WriteResponse(int id, string? result)
    {
        JsonWriter.WriteStartObject();
        JsonWriter.WriteString("jsonrpc", "2.0");
        if (result is not null)
            JsonWriter.WriteString("result", result);
        else
            JsonWriter.WriteNull("result");
        JsonWriter.WriteNumber("id", id);
        JsonWriter.WriteEndObject();
        JsonWriter.Flush();
    }

    private void WriteKeyholeValue(ref Keyhole keyhole)
    {
        // String and Boolean do not use format strings.
        switch (keyhole.Type)
        {
            case KeyholeType.String:
                JsonWriter.WriteStringValue(keyhole.String);
                return;
            case KeyholeType.Boolean:
                JsonWriter.WriteBooleanValue(keyhole.Boolean);
                return;
        }

        JsonWriter.Flush();
        Encoding.UTF8.GetBytes("\"", BufferWriter);
        WriteKeyholeToRawBuffer(ref keyhole);
        Encoding.UTF8.GetBytes("\"", BufferWriter);
    }

    private void WriteKeyholeToRawBuffer(ref Keyhole keyhole)
    {
        // TODO: Does Writer.GetSpan() need a length?  What's the max length of all T's?
        int length = 0;
        switch (keyhole.Type)
        {
            case KeyholeType.Integer:
                while (!keyhole.Integer.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.Long:
                while (!keyhole.Long.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.Float:
                while (!keyhole.Float.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.Double:
                while (!keyhole.Double.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.Decimal:
                while (!keyhole.Decimal.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.DateTime:
                while (!keyhole.DateTime.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.DateOnly:
                while (!keyhole.DateOnly.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.TimeSpan:
                while (!keyhole.TimeSpan.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.TimeOnly:
                while (!keyhole.TimeOnly.TryFormat(BufferWriter.GetSpan(), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.Color:
                while (!keyhole.Color.TryFormat(BufferWriter.GetSpan(9), out length, keyhole.Format))
                    BufferWriter.GrowBuffer();
                break;
            case KeyholeType.Uri:
                // TODO: Fix memory allocation and support format string?
                Encoding.UTF8.GetBytes(keyhole.Uri!.ToString(), BufferWriter);
                break;
        }
        BufferWriter.Advance(length);
    }

    public bool TryReset()
    {
        writers.TryReset();
        return true;
    }

    public void Dispose()
    {
        pool.Return(writers);
    }

    private class WritersHolder : IDisposable, IResettable
    {
        public CopyToGrowBufferWriter BufferWriter { get; init; }
        public Utf8JsonWriter JsonWriter { get; init; } 

        public WritersHolder()
        {
            BufferWriter = new CopyToGrowBufferWriter(bufferSize: 1024);
            JsonWriter = new Utf8JsonWriter(BufferWriter);
        }

        public bool TryReset()
        {
            BufferWriter.TryReset();
            JsonWriter.Reset(BufferWriter);
            return true;
        }

        public void Dispose()
        {
            BufferWriter.Dispose();
            JsonWriter.Dispose();
        }
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