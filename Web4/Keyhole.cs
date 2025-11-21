using System.Drawing;

namespace Web4;

public struct Keyhole
{
    private string key;         // 8 bytes
    private object? reference;  // 8 bytes
    private long value1;        // 8 bytes
    private int value2;         // 4 bytes
    private KeyholeType type;   // 4 bytes
    private string? format;     // 8 bytes

    public string Key { readonly get => key; set => key = value; }
    public KeyholeType Type { readonly get => type; set => type = value; }
    public string? Format { readonly get => format; set => format = value; }

    // --- shared backing field: reference ---
    // These properties all use `reference` as their backing field.  Since each keyhole
    // can only represent a single type at a time we can save a great deal of 
    // wasted RAM and boost memory locality of keyhole buffers by sharing a single backing store.
    public string? StringLiteral    { set => this.reference = value; get => reference as string; }
    public string? String           { set => this.reference = value; get => reference as string; }
    public Uri? Uri                 { set => this.reference = value; get => reference as Uri; }
    public string? Expression       { set => this.reference = value; get => reference as string; }
    public object? Tag              { set => this.reference = value; get => reference; }

    // --- backing field: value1 ---
    // These properties all use `value1` as their backing field.  Since each keyhole
    // can only represent a single type at a time we can save a great deal of 
    // wasted RAM and boost memory locality of keyhole buffers by sharing a single backing store 
    // and converting to and from a 64 bit number.  The primary use case is to check
    // equality between two keyholes and we can bypass type conversion and compare 
    // value1's directly (as long at the types match too).
    public bool Boolean { readonly get => value1 != 0; set => value1 = value ? 1 : 0; }
    public Color Color { readonly get => Color.FromArgb((int)value1); set => value1 = value.ToArgb(); }
    public int Integer { readonly get => (int)value1; set => value1 = value; }
    public long Long { readonly get => value1; set => value1 = value; }
    public float Float { readonly get => (float)BitConverter.Int64BitsToDouble(value1); set => value1 = BitConverter.DoubleToInt64Bits(value); }
    public double Double { readonly get => BitConverter.Int64BitsToDouble(value1); set => value1 = BitConverter.DoubleToInt64Bits(value); }
    public decimal Decimal { readonly get => (decimal)BitConverter.Int64BitsToDouble(value1); set => value1 = BitConverter.DoubleToInt64Bits((double)value); } // Note: lossy precision here
    public DateTime DateTime { readonly get => new(value1); set => value1 = value.Ticks; }
    public DateOnly DateOnly { readonly get => DateOnly.FromDayNumber((int)value1); set => value1 = value.DayNumber; }
    public TimeSpan TimeSpan { readonly get => new(value1); set => value1 = value.Ticks; }
    public TimeOnly TimeOnly { readonly get => new(value1); set => value1 = value.Ticks; }

    // --- backing field: value2 ---
    // These are "helper properties" and most use `value2` as their backing field.  
    // Like the properties that use value1, they aim to conserve memory width in keyhole 
    // buffers by reusing one backing field across a number of properties that are only 
    // used depending on the keyhole type.
    public Range Sequence => SequenceStart..(SequenceStart + SequenceLength);
    public int SequenceStart { readonly get => value2; set => value2 = value; }
    public int SequenceLength { readonly get => (int)value1; set => value1 = value; }
    public bool IsValueAnAttribute { readonly get => value2 == -1; set => value2 = value ? -1 : 0; }

    public int ParentStart { readonly get => value2; set => value2 = value; }
    public static bool Equals(ref Keyhole left, ref Keyhole right)
        => left.Type == right.Type && left.Type switch
        {
            KeyholeType.StringLiteral
                => Object.ReferenceEquals(left.reference, right.reference),
            KeyholeType.String
                => left.reference == right.reference,
            KeyholeType.Uri
                => left.reference == right.reference && left.Format == right.Format,
            KeyholeType.Boolean
                => left.value1 == right.value1,
            KeyholeType.Integer or
            KeyholeType.Long or
            KeyholeType.Float or
            KeyholeType.Double or
            KeyholeType.Decimal or
            KeyholeType.DateTime or
            KeyholeType.DateOnly or
            KeyholeType.TimeSpan or
            KeyholeType.TimeOnly or
            KeyholeType.Color
                => left.value1 == right.value1 && left.Format == right.Format,
            _ => throw new NotSupportedException()
        };
}