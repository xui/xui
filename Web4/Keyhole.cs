using System.Drawing;

namespace Web4;

public struct Keyhole
{
    private long value; // 64 bits
    private object? reference; // 64 bits
    public string Key { get; set; } // 64 bits
    public FormatType Type { get; set; } // 32 bits
    public string? Format { get; set; } // 64 bits
    public int Length { get; set; } // 32 bits

    /// <summary>
    /// Helper property to make the code in DiffUtil read easier.
    /// </summary>
    public readonly string? StringLiteral => String;

    /// <summary>
    /// Helper property to make the code in DiffUtil read easier.
    /// </summary>
    public readonly Range ChildIndices => Integer..(Integer + Length);

    /// <summary>
    /// Helper property to make the code in DiffUtil read easier
    /// </summary>
    public readonly bool IsMemberOfHtmlAttribute => Length > 0;

    /// <summary>
    /// Value-based keyholes don't need to track a range and can reuse this 
    /// backing field as an index to its parent attribute.
    /// </summary>
    public int AttributeStartIndex { readonly get => Length; set => Length = value; }

    public string? String { readonly get => reference as string; set => this.reference = value; }
    public bool Boolean { readonly get => value != 0; set => this.value = value ? 1 : 0; }
    public Color Color { readonly get => Color.FromArgb((int)value); set => this.value = value.ToArgb(); }
    public Uri? Uri { readonly get => reference as Uri; set => this.reference = value; }
    public int Integer { readonly get => (int)value; set => this.value = value; }
    public long Long { readonly get => value; set => this.value = value; }
    public float Float { readonly get => (float)BitConverter.Int64BitsToDouble(value); set => this.value = BitConverter.DoubleToInt64Bits(value); }
    public double Double { readonly get => BitConverter.Int64BitsToDouble(value); set => this.value = BitConverter.DoubleToInt64Bits(value); }
    public decimal Decimal { readonly get => (decimal)BitConverter.Int64BitsToDouble(value); set => this.value = BitConverter.DoubleToInt64Bits((double)value); } // Note: lossy precision here
    public DateTime DateTime { readonly get => new(value); set => this.value = value.Ticks; }
    public DateOnly DateOnly { readonly get => DateOnly.FromDayNumber((int)value); set => this.value = value.DayNumber; }
    public TimeSpan TimeSpan { readonly get => new(value); set => this.value = value.Ticks; }
    public TimeOnly TimeOnly { readonly get => new(value); set => this.value = value.Ticks; }

    public static bool operator ==(Keyhole c1, Keyhole c2) => Equals(ref c1, ref c2);
    public static bool operator !=(Keyhole left, Keyhole right) => !Equals(ref left, ref right);
    public static bool Equals(ref Keyhole left, ref Keyhole right)
        => left.Type == right.Type && left.Type switch
        {
            FormatType.StringLiteral or
            FormatType.String
                => left.reference == right.reference,
            FormatType.Uri
                => left.reference == right.reference && left.Format == right.Format,
            FormatType.Boolean
                => left.value == right.value,
            FormatType.Integer or
            FormatType.Long or
            FormatType.Float or
            FormatType.Double or
            FormatType.Decimal or
            FormatType.DateTime or
            FormatType.DateOnly or
            FormatType.TimeSpan or
            FormatType.TimeOnly or
            FormatType.Color
                => left.value == right.value && left.Format == right.Format,
            _ => throw new NotSupportedException()
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