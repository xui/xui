using System.Buffers;
using System.Drawing;
using System.Text;

namespace Web4;

public struct JsonRpcWriter : IDisposable
{
    private int bufferSize;
    private byte[]? buffer = null;
    private int cursor = 0;

    public JsonRpcWriter() : this(1024)
    {
    }

    public JsonRpcWriter(int bufferSize = 1024)
    {
        this.bufferSize = bufferSize;
    }

    public readonly ReadOnlyMemory<byte>? Memory => buffer?.AsMemory(..cursor);

    public void WriteRpc(string method, ref Keyhole keyhole)
    {
        var isBool = keyhole.Type == KeyholeType.Boolean;
        StartRpcMessage(method, keyhole.Key, useQuotes: !isBool);
        Write(ref keyhole);
        EndRpcMessage(!isBool);
    }

    public void WriteRpc(string method, string key, Span<Keyhole> keyholes)
    {
        StartRpcMessage(method, key);
        for (int i = 0; i < keyholes.Length; i++)
            Write(ref keyholes[i]);
        EndRpcMessage();
    }

    private void StartRpcMessage(string method, string key, bool useQuotes = true)
    {
        Write(cursor == 0 ? "[" : ",");

        Write("""
            {"jsonrpc":"2.0","method":"
            """);
        Write(method);
        Write("""
            ","params":["
            """);
        Write(key);
        Write(useQuotes ? "\",\"" : "\",");
    }

    private void EndRpcMessage(bool useQuotes = true)
    {
        Write(useQuotes ? "\"]}" : "]}");
    }

    private bool Write(ref Keyhole keyhole)
    {
        return keyhole.Type switch
        {
            KeyholeType.StringLiteral => Write(keyhole.StringLiteral ?? string.Empty),
            KeyholeType.String => Write(keyhole.String ?? string.Empty),
            KeyholeType.Boolean => Write(keyhole.Boolean ? "true" : "false"),
            KeyholeType.Color => Write(keyhole.Color, keyhole.Format),
            KeyholeType.Uri => Write(keyhole.Uri!.ToString()), // TODO: Fix memory allocation!
            KeyholeType.Integer => Write(keyhole.Integer, keyhole.Format),
            KeyholeType.Long => Write(keyhole.Long, keyhole.Format),
            KeyholeType.Float => Write(keyhole.Float, keyhole.Format),
            KeyholeType.Double => Write(keyhole.Double, keyhole.Format),
            KeyholeType.Decimal => Write(keyhole.Decimal, keyhole.Format),
            KeyholeType.DateTime => Write(keyhole.DateTime, keyhole.Format),
            KeyholeType.DateOnly => Write(keyhole.DateOnly, keyhole.Format),
            KeyholeType.TimeSpan => Write(keyhole.TimeSpan, keyhole.Format),
            KeyholeType.TimeOnly => Write(keyhole.TimeOnly, keyhole.Format),
            _ => throw new NotImplementedException()
        };
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

    private bool Write(Color value, string? format = null)
    {
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
        Span<byte> destination = buffer.AsSpan(cursor..);

        if (value.TryFormat(destination, out var bytesWritten, format))
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

    public void EndBatch()
    {
        if (cursor > 0)
            Write("]");
    }

    private void GrowBuffer(int sizeHint)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        // Growing by small increments is wasteful.  Buffer-growth should at least double.
        // Skip the gradual doubling if we know it won't be enough.
        bufferSize = Math.Max(sizeHint, buffer.Length) + buffer.Length;

        // TODO: Should this be a configuration somewhere?
        if (bufferSize > 100_000_000)
            throw new ApplicationException("Max buffer size exceeded.");

        var oldBuffer = buffer;
        var newBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        oldBuffer.CopyTo(newBuffer, 0);
        buffer = newBuffer;
        ArrayPool<byte>.Shared.Return(oldBuffer);
    }

    public void Dispose()
    {
        if (buffer is not null)
            ArrayPool<byte>.Shared.Return(buffer);
    }
}