using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace Web4;

internal class WebSocketWriter(WebSocket webSocket, int bufferSize = 1024)
{
    private byte[]? buffer = null;
    private int cursor = 0;
    private bool pendingFlush = false;

    public async ValueTask Write(string value)
    {
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
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
        buffer ??= ArrayPool<byte>.Shared.Rent(bufferSize);
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

    public async ValueTask WriteRpc(string method, params string[] args)
    {
        await Write("""
            {"jsonrpc":"2.0","method":"
            """);
        await Write(method);
        await Write("""
            ","params":[
            """);
        for (int i = 0; i < args.Length; i++)
        {
            await Write(i == 0 ? "\"" : ",\"");
            await Write(args[i]);
            await Write("\"");
        }
        await Write("]}");
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

    public ValueTask Flush(CancellationToken cancel = default) 
        => Flush(true, cancel);

    private async ValueTask Flush(bool endOfMessage = true, CancellationToken cancel = default)
    {
        if (!pendingFlush)
            return;
            
        ArgumentNullException.ThrowIfNull(buffer);

        await webSocket.SendAsync(
            buffer.AsMemory(..cursor), 
            WebSocketMessageType.Text, 
            endOfMessage, 
            cancel
        );

        cursor = 0;
        pendingFlush = false;
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = null;
    }
}