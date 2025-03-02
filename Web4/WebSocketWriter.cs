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
        else
        {
            // Write failed.  Try flushing first, if that doesn't work, rent a bigger buffer.
            if (cursor > 0)
            {
                await Flush(endOfMessage: false);
                cursor = 0;

                // Try again
                await Write(value);
            }
            else
            {
                bufferSize *= 2;

                // TODO: Should this be a configuration somewhere?
                if (bufferSize > 1_000_000)
                    throw new ApplicationException("Max buffer size exceeded.");

                ArrayPool<byte>.Shared.Return(buffer);
                buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                // Try again
                await Write(value);
            }
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
        else
        {
            // Write failed.  Try flushing first, if that doesn't work, rent a bigger buffer.
            if (cursor > 0)
            {
                await Flush(endOfMessage: false);
                cursor = 0;

                // Try again
                await Write(value, format);
            }
            else
            {
                bufferSize *= 2;

                // TODO: Should this be a configuration somewhere?
                if (bufferSize > 1_000_000)
                    throw new ApplicationException("Max buffer size exceeded.");

                ArrayPool<byte>.Shared.Return(buffer);
                buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                // Try again
                await Write(value, format);
            }
        }
    }

    public ValueTask Flush() => Flush(true);
    private ValueTask Flush(bool endOfMessage = true)
    {
        pendingFlush = false;
        return webSocket.SendAsync(
            buffer.AsMemory(..cursor), 
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