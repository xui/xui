using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using Web4;

internal class WebSocketWriter(
    WebSocket webSocket, 
    int bufferSize = 1024,
    CancellationToken cancellationToken = default)
        : IDisposable
{
    private byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
    private int cursor = 0;
    private bool pendingFlush = false;

    public async ValueTask Write(string value)
    {
        Span<byte> destination = buffer.AsSpan(cursor..);
        if (Encoding.UTF8.TryGetBytes(value, destination, out var bytesWritten))
        {
            cursor += bytesWritten;
            pendingFlush = true;
        }
        else if (cursor > 0)
        {
            await Flush(endOfMessage: false);
            await Write(value);
        }
        else
        {
            GrowBuffer(value.Length);
            await Write(value);
        }
    }

    public async ValueTask Write<T>(T value, string? format = null) where T : IUtf8SpanFormattable
    {
        Span<byte> destination = buffer.AsSpan(cursor..);
        if (value.TryFormat(destination, out var bytesWritten, format, null))
        {
            cursor += bytesWritten;
            pendingFlush = true;
        }
        else if (cursor > 0)
        {
            await Flush(endOfMessage: false);
            await Write(value, format);
        }
        else
        {
            GrowBuffer(1);
            await Write(value, format);
        }
    }

    private void GrowBuffer(int sizeHint)
    {
        // Growing by small increments is wasteful.  Buffer-growth should at least double.
        // Skip the gradual doubling if we know it won't be enough.
        int growBy = Math.Max(sizeHint, bufferSize);
        bufferSize += growBy;

        // TODO: Should this be a configuration somewhere?
        if (bufferSize > 100_000_000)
            throw new ApplicationException("Max buffer size exceeded.");

        var oldBuffer = buffer;
        var newBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        oldBuffer.CopyTo(newBuffer, 0);
        buffer = newBuffer;
        ArrayPool<byte>.Shared.Return(oldBuffer);
    }

    public ValueTask Flush() => Flush(true);
    private ValueTask Flush(bool endOfMessage = true)
    {
        var range = 0..cursor;
        cursor = 0;
        pendingFlush = false;

        return webSocket.SendAsync(
            buffer.AsMemory(range), 
            WebSocketMessageType.Text, 
            endOfMessage, 
            cancellationToken
        );
    }

    public void Dispose()
    {
        if (pendingFlush)
            throw new ApplicationException("WebSocketWriter must be Flush()'d before it is Dispose()'d");
        ArrayPool<byte>.Shared.Return(buffer);
    }
}