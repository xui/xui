using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Web4.JsonRpc;

public partial class JsonRpcWriter : IDisposable
{
    [ThreadStatic]
    private static JsonRpcWriter? threadStaticWriter;
    private readonly ArrayBufferWriter<byte> bufferWriter; // TODO: Build a custom BufferWriter that constructs a ReadOnlySequence<T>
    private readonly Utf8JsonWriter jsonWriter;
    private ChannelWriter<ReadOnlySequence<byte>>? flusher = null;
    private FlushOnAwait? flushOnAwait;
    private bool isBatch = false;

    private JsonRpcWriter()
    {
        bufferWriter = new();
        jsonWriter = new(bufferWriter);
    }

    public static JsonRpcWriter Current(ChannelWriter<ReadOnlySequence<byte>> flusher)
    {
        var writer = threadStaticWriter ??= new();
        writer.flusher = flusher;

        if (SynchronizationContext.Current is FlushOnAwait)
            writer.isBatch = true;

        return writer;
    }

    public FlushOnDispose UseBatchForThisScope(bool continueOnCapturedContext = false)
    {
        if (continueOnCapturedContext)
        {
            flushOnAwait ??= new();
            flushOnAwait.Flusher = flusher;
            SynchronizationContext.SetSynchronizationContext(flushOnAwait);
        }

        if (!isBatch)
        {
            if (bufferWriter.WrittenCount > 0)
                throw new InvalidOperationException("Cannot switch to batch.  Buffer already written to.");
            isBatch = true;
        }

        return new FlushOnDispose(this, continueOnCapturedContext);
    }

    public void Flush()
    {
        jsonWriter.Flush();

        if (isBatch && bufferWriter.WrittenCount > 0)
        {
            jsonWriter.WriteEndArray();
            jsonWriter.Flush();
        }

        isBatch = false;

        if (bufferWriter.WrittenCount == 0)
            return;

        // TODO: This line will be replaced when ArrayBufferWriter is swapped for PooledBufferWriter.
        var buffer = new ReadOnlySequence<byte>(bufferWriter.WrittenMemory);

        bufferWriter.ResetWrittenCount();
        jsonWriter.Reset(bufferWriter);

        if (flusher is null)
            throw new InvalidOperationException("🛑 Trying to flush when flusher is null.  This should be impossible.  Needs investigating.");
        while (!flusher.TryWrite(buffer)) ;
    }

    public void WriteNotification(string method)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(ValueTuple<string, string, string> method)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification<T>(string method, T param)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteTValue(param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification<T>(ValueTuple<string, string, string> method, T param)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteTValue(param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(string method, string value1, params Span<string> @values)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        jsonWriter.WriteStringValue(value1);
        foreach (var value in values)
            jsonWriter.WriteStringValue(value);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(string method, params Span<object> @values)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        foreach (var value in values)
            jsonWriter.WriteStringValue(value.ToString());
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(ValueTuple<string, string, string> method, ref Keyhole param)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteKeyholeValue(ref param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(ValueTuple<string, string, string> method, Span<Keyhole> keyholes, bool includeSentinels, string? transition = null)
        => WriteNotification(method, null, keyholes, includeSentinels, transition);

    public void WriteNotification(ValueTuple<string, string, string> method, string? param1, Span<Keyhole> param2, bool includeSentinels, string? transition = null)
    {
        OnMessageBegin();

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

        OnMessageEnd();
    }

    public void WriteRequest(int id)
    {
        // TODO: Implement
    }

    public void WriteResponse(int id)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");
        jsonWriter.WriteNull("result");
        jsonWriter.WriteNumber("id", id);
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();

        OnMessageEnd();
    }

    public void WriteResponse<T>(int id, T result)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");
        jsonWriter.WritePropertyName("result");
        WriteTValue(result);
        jsonWriter.WriteNumber("id", id);
        jsonWriter.WriteEndObject();
        jsonWriter.Flush();

        OnMessageEnd();
    }

    private void OnMessageBegin()
    {
        if (isBatch && jsonWriter.BytesCommitted + jsonWriter.BytesPending + bufferWriter.WrittenCount == 0)
            jsonWriter.WriteStartArray();
    }

    private void OnMessageEnd()
    {
        if (!isBatch)
            Flush();
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
        int length = 0;
        int sizeHint = 30;
        switch (keyhole.Type)
        {
            case KeyholeType.Integer:
                while (!keyhole.Integer.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Long:
                while (!keyhole.Long.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Float:
                while (!keyhole.Float.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Double:
                while (!keyhole.Double.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Decimal:
                while (!keyhole.Decimal.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.DateTime:
                while (!keyhole.DateTime.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.DateOnly:
                while (!keyhole.DateOnly.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.TimeSpan:
                while (!keyhole.TimeSpan.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.TimeOnly:
                while (!keyhole.TimeOnly.TryFormat(bufferWriter.GetSpan(), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Color:
                while (!keyhole.Color.TryFormat(bufferWriter.GetSpan(9), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Uri:
                // TODO: Fix memory allocation and support format string?
                Encoding.UTF8.GetBytes(keyhole.Uri!.ToString(), bufferWriter);
                break;
        }
        bufferWriter.Advance(length);
    }

    private static void GrowSizeHint(ref int sizeHint)
    {
        sizeHint *= 2;
        if (sizeHint > (2 ^ 20)) // 1MB
            throw new NotSupportedException("🛑 It seems a keyhole value with a format string needed a buffer > 1MB.  Probably misuse?  Needs investigation.");
    }

    public void Dispose()
    {
        jsonWriter.Dispose();
    }
}