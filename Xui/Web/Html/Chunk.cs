using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web;

public struct Chunk
{
    public int Id;
    public FormatType Type;
    public string? String;
    public int? Integer;
    public long? Long;
    public float? Float;
    public double? Double;
    public decimal? Decimal;
    public bool? Boolean;
    public DateTime? DateTime;
    public TimeSpan? TimeSpan;
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
            case FormatType.Long:
                return c1.Long == c2.Long && c1.Format == c2.Format;
            case FormatType.Float:
                return c1.Float == c2.Float && c1.Format == c2.Format;
            case FormatType.Double:
                return c1.Double == c2.Double && c1.Format == c2.Format;
            case FormatType.Decimal:
                return c1.Decimal == c2.Decimal && c1.Format == c2.Format;
            case FormatType.DateTime:
                return c1.DateTime == c2.DateTime && c1.Format == c2.Format;
            case FormatType.TimeSpan:
                return c1.TimeSpan == c2.TimeSpan && c1.Format == c2.Format;
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

    public void Write(IBufferWriter<byte> writer)
    {
        Span<byte> destination;
        int length;
        switch (Type)
        {
            case FormatType.StringLiteral:
                destination = writer.GetSpan(String!.Length);
                length = Encoding.UTF8.GetBytes(String, destination);
                writer.Advance(length);
                break;
            case FormatType.String:
                // string has no formatters (and its alignment isn't helpful in HTML)
                destination = writer.GetSpan(String!.Length);
                length = Encoding.UTF8.GetBytes(String, destination);
                writer.Advance(length);
                break;
            case FormatType.Integer:
                destination = writer.GetSpan();
                Integer!.Value.TryFormat(destination, out length, Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Long:
                destination = writer.GetSpan();
                Long!.Value.TryFormat(destination, out length, Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Float:
                destination = writer.GetSpan();
                Float!.Value.TryFormat(destination, out length, Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Double:
                destination = writer.GetSpan();
                Double!.Value.TryFormat(destination, out length, Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Decimal:
                destination = writer.GetSpan();
                Decimal!.Value.TryFormat(destination, out length, Format ?? string.Empty);
                writer.Advance(length);
                break;
            case FormatType.Boolean:
                // bool has no formatters
                var value = Boolean!.Value ? System.Boolean.TrueString : System.Boolean.FalseString;
                destination = writer.GetSpan(value.Length);
                length = Encoding.UTF8.GetBytes(value, destination);
                writer.Advance(length);
                break;
            case FormatType.DateTime:
                destination = writer.GetSpan();
                DateTime!.Value.TryFormat(destination, out length, Format);
                writer.Advance(length);
                break;
            case FormatType.TimeSpan:
                destination = writer.GetSpan();
                TimeSpan!.Value.TryFormat(destination, out length, Format);
                writer.Advance(length);
                break;
            case FormatType.View:
            case FormatType.HtmlString:
                // For writing, HtmlString is a no-op since all their children are already unrolled.
                // When Recomposing, HtmlString might "compose" a range of slots instead.
                break;
            case FormatType.Action:
            case FormatType.ActionAsync:
            case FormatType.ActionEvent:
            case FormatType.ActionEventAsync:
                // No values to write.  The parent iterator might output sentinels.
                break;
            default:
                throw new Exception($"Unsupported type: {Type}");
        }
    }
}