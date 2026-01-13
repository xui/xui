using System.Runtime.CompilerServices;
using System.Text;

namespace Web4;

public class KeyCursor
{
    private static readonly byte[] KEY_PREFIX = "key"u8.ToArray();

    private int currentDepth;
    private int[] levels = new int[100]; // TODO: Make this grow gracefully
    private int[] widths = new int[100]; // TODO: Make this grow gracefully

    public int CurrentLength { get; private set; } = KEY_PREFIX.Length;

    public KeyCursor() => Reset();

    public void Reset()
    {
        currentDepth = -1;
        CurrentLength = KEY_PREFIX.Length;
    }

    public void MoveNext()
    {
        levels[currentDepth]++;
    }

    public void MoveDown(int numberOfSiblings)
    {
        int numDigits = GetWidth(numberOfSiblings);
        currentDepth++;
        levels[currentDepth] = -1;
        widths[currentDepth] = numDigits;

        CurrentLength += numDigits;
    }

    public void MoveUp()
    {
        CurrentLength -= widths[currentDepth];
        currentDepth--;
    }

    public string Current
    {
        get
        {
            var buffer = new byte[CurrentLength]; // TODO: malloc
            Write(buffer);
            // return buffer;
            return Encoding.UTF8.GetString(buffer);
        }
    }

    public void Write(Span<byte> buffer)
    {
        KEY_PREFIX.CopyTo(buffer);
        for (int i = KEY_PREFIX.Length, level = 0; level <= currentDepth; level++)
        {
            int sibling = levels[level];
            int width = widths[level];
            Span<byte> dest = buffer[i..(i + width)];
            i += width;

            if (width >= 7) dest[^7] = ToHexChar(sibling >> 24);
            if (width >= 6) dest[^6] = ToHexChar(sibling >> 20);
            if (width >= 5) dest[^5] = ToHexChar(sibling >> 16);
            if (width >= 4) dest[^4] = ToHexChar(sibling >> 12);
            if (width >= 3) dest[^3] = ToHexChar(sibling >> 8);
            if (width >= 2) dest[^2] = ToHexChar(sibling >> 4);
            if (width >= 1) dest[^1] = ToHexChar(sibling);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToHexChar(int value)
    {
        value &= 0b1111;
        return value switch
        {
            < 10 => (byte)(value + '0'),
            _ => (byte)(value - 10 + 'a')
        };
    }

    private static int GetWidth(int count) => count switch
    {
        <= 0x10 << 1 => 1,
        <= 0x10 << 4 => 2,
        <= 0x10 << 8 => 3,
        <= 0x10 << 12 => 4,
        <= 0x10 << 16 => 5,
        <= 0x10 << 20 => 6,
        <= 0x10 << 24 => 7,
        _ => throw new Exception($"Too many siblings: {count}")
    };
}