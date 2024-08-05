using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Routing.Tree;

namespace Xui.Web.Html;

internal struct Chunk
{
    public int Id;
    public FormatType Type;
    public string? String;
    public int? Integer;
    public bool? Boolean;
    public DateTime? DateTime;
    public IView? View;
    public string? Format;
    public Action Action;
    public Action<Event> ActionEvent;
    public Func<Task> ActionAsync;
    public Func<Event, Task> ActionEventAsync;

    public static bool operator ==(Chunk c1, Chunk c2)
    {
        if (c1.Type != c2.Type)
            return false;

        switch (c1.Type)
        {
            case FormatType.StringLiteral:
                return c1.String == c2.String;
            case FormatType.String:
                return c1.String == c2.String && c1.Format == c2.Format;
            case FormatType.Integer:
                return c1.Integer == c2.Integer && c1.Format == c2.Format;
            case FormatType.DateTime:
                return c1.DateTime == c2.DateTime && c1.Format == c2.Format;
            case FormatType.Boolean:
                return c1.Boolean == c2.Boolean && c1.Format == c2.Format;
            case FormatType.View:
                return c1.View == c2.View;
            case FormatType.HtmlString:
                // no-op
                return true;
        }

        return true;
    }

    public static bool operator !=(Chunk c1, Chunk c2)
    {
        return !(c1 == c2);
    }

    public override readonly bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    internal readonly void Append(StringBuilder builder)
    {
        switch (this.Type)
        {
            case FormatType.StringLiteral:
                builder.Append(this.String);
                break;
            case FormatType.String:
                if (this.Format is null)
                    builder.Append(this.String);
                else
                    builder.AppendFormat($"{{0:{this.Format}}}", this.String);
                break;
            case FormatType.Integer:
                if (this.Format is null)
                    builder.Append(this.Integer);
                else
                    builder.AppendFormat($"{{0:{this.Format}}}", this.Integer);
                break;
            case FormatType.Boolean:
                if (this.Format is null)
                    builder.Append(this.Boolean);
                else
                    builder.AppendFormat($"{{0:{this.Format}}}", this.Boolean);
                break;
            case FormatType.DateTime:
                if (this.Format is null)
                    builder.Append(this.DateTime);
                else
                    builder.AppendFormat($"{{0:{this.Format}}}", this.DateTime);
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
                throw new Exception($"Unsupported type: {this.Type}");
        }
    }
}

internal static class ChunkExtensions
{
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
                // TODO: Support chunk.Format
                destination = writer.GetSpan(chunk.String!.Length);
                length = Encoding.UTF8.GetBytes(chunk.String, destination);
                writer.Advance(length);
                break;
            case FormatType.Integer:
                destination = writer.GetSpan();
                chunk.Integer!.Value.TryFormat(destination, out length, chunk.Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Boolean:
                // bool has no custom formatters
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
}