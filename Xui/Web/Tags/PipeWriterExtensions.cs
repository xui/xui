using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Xui.Web.Html;

internal static class PipeWriterExtensions
{
    public static ValueTask<FlushResult> WriteAsync(this PipeWriter writer, ref HtmlString htmlString, CancellationToken cancellationToken = default)
    {
        return htmlString.WriteAsync(writer, cancellationToken);
    }

    public static void Write(this PipeWriter writer, int value)
    {
        var destination = writer.GetSpan();
        value.TryFormat(destination, out int length);
        writer.Advance(length);
    }

    public static void WriteStringLiteral(this PipeWriter writer, string value)
    {
        // TODO: Cache this.
        ReadOnlyMemory<byte> memory = new(Encoding.UTF8.GetBytes(value));
        writer.Write(memory.Span);
    }

    public static ValueTask SendAsync(this WebSocket webSocket, StringBuilder output)
    {
        int length = output.Length;
        using (var owner = MemoryPool<byte>.Shared.Rent(length))
        {
            var memory = owner.Memory;
            Copy(output, memory);
            return webSocket!.SendAsync(
                memory[..length],
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }

    private static void Copy(StringBuilder source, Memory<byte> destination)
    {
        int position = 0;
        foreach (var chunk in source.GetChunks())
        {
            position += Encoding.UTF8.GetBytes(chunk.Span, destination[position..].Span);
        }
    }
}