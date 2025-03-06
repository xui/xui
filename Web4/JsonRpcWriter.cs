using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace Web4;

internal struct JsonRpcWriter(int bufferSize = 1024) : IDisposable
{
    private byte[]? buffer = null;
    private int cursor = 0;
    private int batchCount = 0;

    public readonly Memory<byte> Memory => buffer.AsMemory(..cursor);

    private void Write(string value)
    {
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
        Span<byte> destination = buffer.AsSpan(cursor..);

        if (cursor + value.Length < buffer.Length)
        {
            var bytesWritten = Encoding.UTF8.GetBytes(value, destination);
            cursor += bytesWritten;
        }
        else
        {
            GrowBuffer(value.Length);
            Write(value);
        }
    }

    private void Write<T>(T value, string? format = null) where T : struct, IUtf8SpanFormattable
    {
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
        Span<byte> destination = buffer.AsSpan(cursor..);

        if (value.TryFormat(destination, out var bytesWritten, format, null))
        {
            cursor += bytesWritten;
        }
        else
        {
            GrowBuffer(1);
            Write(value, format);
        }
    }

    public void BeginBatch() => Write("[");
    public void EndBatch() => Write("]");

    public void Reset()
    {
        cursor = 0;
        batchCount = 0;
    }

    public void WriteRpc(string method, params Span<string> args)
    {
        if (batchCount++ > 0)
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