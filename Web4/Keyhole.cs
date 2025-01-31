using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Web4;

public struct Keyhole
{
    public string Key;
    public FormatType Type;
    public string? String;
    public bool? Boolean;
    public Color? Color;
    public Uri? Uri;
    public int? Integer;
    public long? Long;
    public float? Float;
    public double? Double;
    public decimal? Decimal;
    public DateTime? DateTime;
    public DateOnly? DateOnly;
    public TimeSpan? TimeSpan;
    public TimeOnly? TimeOnly;
    public string? Format;

    public static bool operator ==(Keyhole c1, Keyhole c2) => Equals(ref c1, ref c2);
    public static bool operator !=(Keyhole left, Keyhole right)
        => !Equals(ref left, ref right);
    public static bool Equals(ref Keyhole left, ref Keyhole right)
        => left.Type == right.Type && left.Type switch
        {
            FormatType.StringLiteral    => left.String == right.String,
            FormatType.String           => left.String == right.String,
            FormatType.Boolean          => left.Boolean == right.Boolean,
            FormatType.Color            => left.Color == right.Color         && left.Format == right.Format,
            FormatType.Uri              => left.Uri == right.Uri             && left.Format == right.Format,
            FormatType.Integer          => left.Integer == right.Integer     && left.Format == right.Format,
            FormatType.Long             => left.Long == right.Long           && left.Format == right.Format,
            FormatType.Float            => left.Float == right.Float         && left.Format == right.Format,
            FormatType.Double           => left.Double == right.Double       && left.Format == right.Format,
            FormatType.Decimal          => left.Decimal == right.Decimal     && left.Format == right.Format,
            FormatType.DateTime         => left.DateTime == right.DateTime   && left.Format == right.Format,
            FormatType.DateOnly         => left.DateOnly == right.DateOnly   && left.Format == right.Format,
            FormatType.TimeSpan         => left.TimeSpan == right.TimeSpan   && left.Format == right.Format,
            FormatType.TimeOnly         => left.TimeOnly == right.TimeOnly   && left.Format == right.Format,
            _ => false
        };

    public override readonly bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}