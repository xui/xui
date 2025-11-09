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
        jsonWriter.WriteString("method", method);
        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification<T>(string method, T param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WriteString("method", method);

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

        jsonWriter.WriteString("method", method);

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

        jsonWriter.WriteString("method", method);

        jsonWriter.WriteStartArray("params");
        foreach (var param in @params)
            jsonWriter.WriteStringValue(param.ToString());
        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetTextNode and SetAttribute
    public void WriteNotification(ValueTuple<string, string, string> method, ref Keyhole param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);

        jsonWriter.WriteStartArray("params");

        if (method.Item3 == ".setAttribute" && param1.Type == KeyholeType.Boolean)
        {
            // HTML treats boolean attributes differently.  Send without quotes.
            jsonWriter.WriteBooleanValue(param1.Boolean);
        }
        else
        {
            jsonWriter.WriteStringValueSegment("", false);
            WriteMutableKeyholeValue(ref param1);
            jsonWriter.WriteStringValueSegment("", true);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetAttribute
    public void WriteNotification(ValueTuple<string, string, string> method, Span<Keyhole> param1)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);

        jsonWriter.WriteStartArray("params");

        WriteAttributeSequence(param1);
        jsonWriter.WriteStringValueSegment("", true);

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetElement
    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, string, string> method, Span<Keyhole> param1, ValueTuple<string, string>? param2 = null)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);

        jsonWriter.WriteStartArray("params");

        WriteHtmlPartial(buffer, param1, includeSentinels: true);
        jsonWriter.WriteStringValueSegment("", true);

        if (param2.HasValue)
        {
            jsonWriter.WriteStringValueSegment(param2.Value.Item1, false);
            jsonWriter.WriteStringValueSegment(param2.Value.Item2, true);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetElement
    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, string, string> method, Span<Keyhole> param1, ValueTuple<string, int> param2, ValueTuple<string, int> param3)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);

        jsonWriter.WriteStartArray("params");

        WriteHtmlPartial(buffer, param1, includeSentinels: true);
        jsonWriter.WriteStringValueSegment("", true);

        Span<char> strInt = stackalloc char[11]; // max int length

        jsonWriter.WriteStringValueSegment(param2.Item1, false);
        if (param2.Item2.TryFormat(strInt, out int length))
            jsonWriter.WriteStringValueSegment(strInt[..length], false);
        jsonWriter.WriteStringValueSegment("", true);

        jsonWriter.WriteStringValueSegment(param3.Item1, false);
        if (param3.Item2.TryFormat(strInt, out length))
            jsonWriter.WriteStringValueSegment(strInt[..length], false);
        jsonWriter.WriteStringValueSegment("", true);

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from AddElement
    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, string, string> method, Span<Keyhole> param1, string param2, ValueTuple<string, int>? param3 = null)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);

        jsonWriter.WriteStartArray("params");

        WriteHtmlPartial(buffer, param1, includeSentinels: true);
        jsonWriter.WriteStringValueSegment("", true);

        jsonWriter.WriteStringValue(param2);

        if (param3 is not null)
        {
            jsonWriter.WriteStringValueSegment(param3.Value.Item1, false);
            Span<char> strInt = stackalloc char[11]; // max int length
            if (param3.Value.Item2.TryFormat(strInt, out int length))
                jsonWriter.WriteStringValueSegment(strInt[..length], false);
            jsonWriter.WriteStringValueSegment("", true);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from RemoveElement
    public void WriteNotification(ValueTuple<string, string, string> method, ValueTuple<string, int>? param1 = null)
    {
        OnMessageBegin();

        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("jsonrpc", "2.0");

        jsonWriter.WritePropertyName("method");
        jsonWriter.WriteStringValueSegment(method.Item1, false);
        jsonWriter.WriteStringValueSegment(method.Item2, false);
        jsonWriter.WriteStringValueSegment(method.Item3, true);

        jsonWriter.WriteStartArray("params");

        if (param1.HasValue)
        {
            jsonWriter.WriteStringValueSegment(param1.Value.Item1, false);
            Span<char> strInt = stackalloc char[11]; // max int length
            if (param1.Value.Item2.TryFormat(strInt, out int length))
                jsonWriter.WriteStringValueSegment(strInt[..length], false);
            jsonWriter.WriteStringValueSegment("", true);
        }

        jsonWriter.WriteEndArray();

        jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    private void WriteHtmlPartial(Keyhole[] buffer, Span<Keyhole> keyholes, bool includeSentinels)
    {
        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            switch (keyhole.Type)
            {
                case KeyholeType.StringLiteral:
                    jsonWriter.WriteStringValueSegment(keyhole.StringLiteral, false);
                    break;
                case KeyholeType.Html:
                    Span<Keyhole> partialHtml = buffer.AsSpan(keyhole.Sequence);
                    WriteHtmlPartial(buffer, partialHtml, includeSentinels);
                    if (includeSentinels)
                    {
                        jsonWriter.WriteStringValueSegment("<!--", false);
                        jsonWriter.WriteStringValueSegment(keyhole.Key, false);
                        jsonWriter.WriteStringValueSegment("-->", false);
                    }
                    break;
                case KeyholeType.Attribute:
                    Span<Keyhole> partialAttr = buffer.AsSpan(keyhole.Sequence);
                    WriteAttributeSequence(partialAttr);
                    jsonWriter.WriteStringValueSegment("", true);
                    break;
                case KeyholeType.EventListener:
                    jsonWriter.WriteStringValueSegment("\"keyholes.", false);
                    jsonWriter.WriteStringValueSegment(keyhole.Key, false);
                    jsonWriter.WriteStringValueSegment(".dispatchEvent(event.trim('", false);
                    jsonWriter.WriteStringValueSegment(keyhole.Format ?? "*", false);
                    jsonWriter.WriteStringValueSegment("'))\" ", false);
                    jsonWriter.WriteStringValueSegment(keyhole.Key, false);
                    break;
                case KeyholeType.Enumerable:
                    int start = keyhole.SequenceStart;
                    int end = keyhole.SequenceLength + start;
                    for (int i2 = start; i2 < end; i2++)
                    {
                        ref var k = ref buffer[i2];
                        Span<Keyhole> partialEnumerable = buffer.AsSpan(k.SequenceStart, k.SequenceLength);
                        WriteHtmlPartial(buffer, partialEnumerable, true);
                        if (includeSentinels)
                        {
                            jsonWriter.WriteStringValueSegment("<!--", false);
                            jsonWriter.WriteStringValueSegment(k.Key, false);
                            jsonWriter.WriteStringValueSegment("-->", false);
                        }
                    }
                    break;
                // The rest are the mutable keyhole values.  They might use format strings.
                default:
                    if (includeSentinels)
                    {
                        jsonWriter.WriteStringValueSegment("<!-- -->", false);
                    }

                    WriteMutableKeyholeValue(ref keyhole);

                    if (includeSentinels)
                    {
                        jsonWriter.WriteStringValueSegment("<!--", false);
                        jsonWriter.WriteStringValueSegment(keyhole.Key, false);
                        jsonWriter.WriteStringValueSegment("-->", false);
                    }
                    break;
            }
        }
    }

    private void WriteAttributeSequence(Span<Keyhole> keyholes)
    {
        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            switch (keyhole.Type)
            {
                case KeyholeType.StringLiteral:
                    jsonWriter.WriteStringValueSegment(keyhole.StringLiteral, false);
                    break;
                // The rest are the mutable keyhole values.  They might use format strings.
                default:
                    WriteMutableKeyholeValue(ref keyhole);
                    break;
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

    private void WriteMutableKeyholeValue(ref Keyhole keyhole)
    {
        // String and Boolean do not use format strings.
        switch (keyhole.Type)
        {
            case KeyholeType.String:
                // Must use jsonWriter to write this string with the proper json encoding.
                jsonWriter.WriteStringValueSegment(keyhole.String, false);
                return;
            case KeyholeType.Boolean:
                jsonWriter.WriteStringValueSegment(keyhole.Boolean ? "true" : "false", false);
                return;
        }

        // All other mutable values might make use of a format string.  
        // Flush the JSON writer and switch to the raw buffer writer.
        // Use IUtf8SpanFormattable.TryFormat() to write without allocating memory.

        jsonWriter.Flush();
        int length = 0;
        int sizeHint = 30;
        switch (keyhole.Type)
        {
            case KeyholeType.Integer:
                while (!keyhole.Integer.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Long:
                while (!keyhole.Long.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Float:
                while (!keyhole.Float.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Double:
                while (!keyhole.Double.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Decimal:
                while (!keyhole.Decimal.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.DateTime:
                while (!keyhole.DateTime.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.DateOnly:
                while (!keyhole.DateOnly.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.TimeSpan:
                while (!keyhole.TimeSpan.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.TimeOnly:
                while (!keyhole.TimeOnly.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Color:
                while (!keyhole.Color.TryFormat(bufferWriter.GetSpan(sizeHint), out length, keyhole.Format))
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