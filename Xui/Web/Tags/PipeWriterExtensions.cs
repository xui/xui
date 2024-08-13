using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;

namespace Xui.Web.Html;

internal static class PipeWriterExtensions
{
    public static void Write(this PipeWriter writer, int value)
    {
        var destination = writer.GetSpan();
        value.TryFormat(destination, out int length);
        writer.Advance(length);
    }

    public static void Write(this PipeWriter writer, ReadOnlySpan<char> value)
    {
        Encoding.UTF8.GetBytes(value, writer);
    }

    public static void Write(this PipeWriter writer, string value)
    {
        Write(writer, value.AsSpan());
    }

    public static void WriteStringLiteral(this PipeWriter writer, string value)
    {
        // TODO: Does it help to cache the UTF16 -> UTF8 encodings?  
        // Maybe not because of SIMD optimizations?
        Write(writer, value);
    }

    public static void Write(this IBufferWriter<byte> writer, ref Chunk chunk)
    {
        Span<byte> destination;
        int length;
        switch (chunk.Type)
        {
            case FormatType.StringLiteral:
                destination = writer.GetSpan(chunk.String!.Length);
                length = Encoding.UTF8.GetBytes(chunk.String, destination);
                writer.Advance(length);
                break;
            case FormatType.String:
                // string has no formatters (and its alignment isn't helpful in HTML)
                destination = writer.GetSpan(chunk.String!.Length);
                length = Encoding.UTF8.GetBytes(chunk.String, destination);
                writer.Advance(length);
                break;
            case FormatType.Integer:
                destination = writer.GetSpan();
                chunk.Integer!.Value.TryFormat(destination, out length, chunk.Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Double:
                destination = writer.GetSpan();
                chunk.Double!.Value.TryFormat(destination, out length, chunk.Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Boolean:
                // bool has no formatters
                var value = chunk.Boolean!.Value ? Boolean.TrueString : Boolean.FalseString;
                destination = writer.GetSpan(value.Length);
                length = Encoding.UTF8.GetBytes(value, destination);
                writer.Advance(length);
                break;
            case FormatType.DateTime:
                destination = writer.GetSpan();
                chunk.DateTime!.Value.TryFormat(destination, out length, chunk.Format);
                writer.Advance(length);
                break;
            case FormatType.View:
            case FormatType.HtmlString:
                // When Composing, HtmlString is a no-op since all their children are already unrolled.
                // When Recomposing, HtmlString must "compose" a range of slots instead.
                break;
            case FormatType.Action:
            case FormatType.ActionAsync:
            case FormatType.ActionEvent:
            case FormatType.ActionEventAsync:
                // No values to write.  The parent iterator might output sentinels.
                break;
            default:
                throw new Exception($"Unsupported type: {chunk.Type}");
        }
    }

    public static void Write(this PipeWriter writer, IEnumerable<Memory<Chunk>> deltas)
    {
        foreach (var delta in deltas)
        {
            ref var chunk = ref delta.Span[0];

            if (delta.Length == 1)
            {
                writer.WriteStringLiteral("slot");
                writer.Write(chunk.Id);
                writer.WriteStringLiteral(".nodeValue='");
                writer.Write(ref chunk);
                writer.WriteStringLiteral("';");
            }
            else
            {
                writer.WriteStringLiteral("replaceNode(slot");
                writer.Write(chunk.Id);
                writer.WriteStringLiteral(",`");
                var span = delta.Span;
                for (int i = 0; i <= delta.Length; i++)
                {
                    var c = span[i];
                    writer.Write(ref c);
                }
                writer.WriteStringLiteral("`);");
            }
        }
    }
}