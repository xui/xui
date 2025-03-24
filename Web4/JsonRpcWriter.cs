using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace Web4;

internal struct JsonRpcWriter(int bufferSize = 1024) : IDisposable
{
    private byte[]? buffer = null;
    private int cursor = 0;

    public int BatchCount { get; private set; } = 0;
    public readonly Memory<byte> Memory => buffer.AsMemory(..cursor);

    public void BeginBatch() => Write("[");
    public void EndBatch() => Write("]");

    public void Reset()
    {
        cursor = 0;
        BatchCount = 0;
    }

    public void WriteRpc(string method, params ReadOnlySpan<string> args)
    {
        if (BatchCount++ > 0)
            Write(",");

        Write("""
            {"jsonrpc":"2.0","method":"
            """);
        Write(method);
        Write("""
            ","params":[
            """);
        for (int i = 0; i < args.Length; i++)
        {
            Write(i == 0 ? "\"" : ",\"");
            Write(args[i]);
            Write("\"");
        }
        Write("]}");
    }

    public void WriteRpc(string method, ref Keyhole keyhole)
    {
        if (BatchCount++ > 0)
            Write(",");

        Write("""
            {"jsonrpc":"2.0","method":"
            """);
        Write(method);
        Write("""
            ","params":["
            """);
        Write(keyhole.Key);
        Write("""
            ","
            """);
        _ = keyhole.Type switch
        {
            FormatType.StringLiteral => Write(keyhole.String!),
            FormatType.String => Write(keyhole.String!),
            FormatType.Boolean => Write(keyhole.Boolean ? "true" : "false"),
            FormatType.Color => Write(keyhole.Color.ToString()), // TODO: Fix memory allocation!  Bonus: format strings would be great here.
            FormatType.Uri => Write(keyhole.Uri!.ToString()), // TODO: Fix memory allocation!
            FormatType.Integer => Write(keyhole.Integer, keyhole.Format),
            FormatType.Long => Write(keyhole.Long, keyhole.Format),
            FormatType.Float => Write(keyhole.Float, keyhole.Format),
            FormatType.Double => Write(keyhole.Double, keyhole.Format),
            FormatType.Decimal => Write(keyhole.Decimal, keyhole.Format),
            FormatType.DateTime => Write(keyhole.DateTime, keyhole.Format),
            FormatType.DateOnly => Write(keyhole.DateOnly, keyhole.Format),
            FormatType.TimeSpan => Write(keyhole.TimeSpan, keyhole.Format),
            FormatType.TimeOnly => Write(keyhole.TimeOnly, keyhole.Format),
            FormatType.Attribute => throw new NotImplementedException(),
            FormatType.EventListener => throw new NotImplementedException(),
            FormatType.View => throw new NotImplementedException(),
            FormatType.Html => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
        Write("\"]}");
    }

    private bool Write(string value)
    {
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
        Span<byte> destination = buffer.AsSpan(cursor..);

        if (cursor + value.Length < buffer.Length)
        {
            var bytesWritten = Encoding.UTF8.GetBytes(value, destination);
            cursor += bytesWritten;
            return true;
        }
        else
        {
            GrowBuffer(value.Length);
            return Write(value);
        }
    }

    private bool Write<T>(T value, string? format = null) where T : struct, IUtf8SpanFormattable
    {
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
        Span<byte> destination = buffer.AsSpan(cursor..);

        if (value.TryFormat(destination, out var bytesWritten, format, null))
        {
            cursor += bytesWritten;
            return true;
        }
        else
        {
            GrowBuffer(1);
            return Write(value, format);
        }
    }

    private void GrowBuffer(int sizeHint)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        // Growing by small increments is wasteful.  Buffer-growth should at least double.
        // Skip the gradual doubling if we know it won't be enough.
        int newSize = Math.Max(sizeHint, buffer.Length) + buffer.Length;

        // TODO: Should this be a configuration somewhere?
        if (newSize > 100_000_000)
            throw new ApplicationException("Max buffer size exceeded.");

        var oldBuffer = buffer;
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        oldBuffer.CopyTo(newBuffer, 0);
        buffer = newBuffer;
        ArrayPool<byte>.Shared.Return(oldBuffer);
    }

    public void Dispose()
    {
        cursor = 0;
        if (buffer is not null)
            ArrayPool<byte>.Shared.Return(buffer);
        buffer = null;
    }
}