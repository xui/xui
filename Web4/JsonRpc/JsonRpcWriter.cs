using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Web4.WebSockets;

namespace Web4.JsonRpc;

public partial class JsonRpcWriter : IDisposable
{
    [ThreadStatic]
    private static JsonRpcWriter? threadStaticWriter;
    private readonly PooledSequenceBufferWriter<byte> bufferWriter;
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

    public FlushOnDispose BatchThisScope(bool continueOnCapturedContext = false)
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

        var buffer = bufferWriter.Sequence;
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

    public void WriteNotification<T>(string method, T param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteTValue(param1);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification<T>(ValueTuple<string, string, string> method, T param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteTValue(param1);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(string method, string param1, params Span<string> @params)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        jsonWriter.WriteStringValue(param1);
        foreach (var param in @params)
            jsonWriter.WriteStringValue(param);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(string method, params Span<object> @params)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        foreach (var param in @params)
            jsonWriter.WriteStringValue(param.ToString());
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(ValueTuple<string, string, string> method, ref Keyhole param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");
        WriteKeyholeValue(ref param1);
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(ValueTuple<string, string, string> method, Span<Keyhole> param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");

        WriteHtmlPartial([], param1, includeSentinels: false);
        jsonWriter.WriteStringValueSegment("", true);

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, string, string> method, Span<Keyhole> param1, string? param2 = null)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");

        WriteHtmlPartial(buffer, param1, includeSentinels: true);
        jsonWriter.WriteStringValueSegment("", true);

        if (param2 is not null)
            jsonWriter.WriteStringValue(param2);

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, string, string> method, string? param1, Span<Keyhole> param2, string? param3 = null)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        WriteMethod(method);

        jsonWriter.WriteStartArray("params");

        if (param1 is not null)
            jsonWriter.WriteStringValue(param1);

        WriteHtmlPartial(buffer, param2, includeSentinels: true);
        jsonWriter.WriteStringValueSegment("", true);

        if (param3 is not null)
            jsonWriter.WriteStringValue(param3);
 
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    private void WriteHtmlPartial(Keyhole[] buffer, Span<Keyhole> keyholes, bool includeSentinels)
    {
        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            if (keyhole.Type == KeyholeType.StringLiteral)
            {
                jsonWriter.WriteStringValueSegment(keyhole.StringLiteral, false);
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
                case KeyholeType.Html:
                case KeyholeType.Attribute:
                    int start = keyhole.SequenceStart;
                    int length = keyhole.SequenceLength;
                    Span<Keyhole> partial = buffer.AsSpan(start, length);
                    WriteHtmlPartial(buffer, partial, includeSentinels);
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