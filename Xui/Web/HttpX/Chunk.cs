using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xui.Web.HttpX;

public struct Chunk
{
    public int Key;
    public int RefId;
    public FormatType Type;
    public string? String;
    public int? Integer;
    public long? Long;
    public float? Float;
    public double? Double;
    public decimal? Decimal;
    public DateTime? DateTime;
    public TimeSpan? TimeSpan;
    public bool? Boolean;
    public IView? View;
    public string? Format;

    public static bool operator ==(Chunk c1, Chunk c2)
    {
        if (c1.Type != c2.Type)
            return false;

        return c1.Type switch
        {
            FormatType.StringLiteral    => c1.String == c2.String,
            FormatType.String           => c1.String == c2.String,
            FormatType.Integer          => c1.Integer == c2.Integer     && c1.Format == c2.Format,
            FormatType.Long             => c1.Long == c2.Long           && c1.Format == c2.Format,
            FormatType.Float            => c1.Float == c2.Float         && c1.Format == c2.Format,
            FormatType.Double           => c1.Double == c2.Double       && c1.Format == c2.Format,
            FormatType.Decimal          => c1.Decimal == c2.Decimal     && c1.Format == c2.Format,
            FormatType.DateTime         => c1.DateTime == c2.DateTime   && c1.Format == c2.Format,
            FormatType.TimeSpan         => c1.TimeSpan == c2.TimeSpan   && c1.Format == c2.Format,
            FormatType.Boolean          => c1.Boolean == c2.Boolean,
            _ => true
        };
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
}