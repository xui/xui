using System.Runtime.CompilerServices;
using System.Text;

namespace Web4;

public class KeyCursor2
{
    private static readonly byte[] KEY_PREFIX = "key"u8.ToArray();
    private Memory<byte> parent = KEY_PREFIX;
    private readonly List<int> levels = [];
    private int currentDepth;

    public KeyCursor2() => Reset();

    public void Reset()
    {
        currentDepth = -1;
        parent = KEY_PREFIX;
    }

    public void MoveNext()
    {
        levels[currentDepth]++;
    }

    public void MoveDown()
    {
        parent = CurrentAsBytes;
        currentDepth++;
        
        if (levels.Count <= currentDepth)
            levels.Add(-1);
        else
            levels[currentDepth] = -1;
    }

    public void MoveUp()
    {
        parent = parent[..parent.Span.LastIndexOf((byte)'_')];
        currentDepth--;
    }

    public string Current => Encoding.UTF8.GetString(CurrentAsBytes);
    public byte[] CurrentAsBytes
    {
        get
        {
            if (currentDepth < 0)
                return KEY_PREFIX;

            int siblingIndex = levels[currentDepth];
            int width = GetWidth(siblingIndex);
            var buffer = new byte[parent.Length + width + 1]; // TODO: malloc
            parent.CopyTo(buffer);
            buffer[parent.Length] = (byte)'_';

            Span<byte> dest = buffer.AsSpan(parent.Length + 1);
            if (width >= 7) dest[^7] = ToHexChar(siblingIndex >> 24);
            if (width >= 6) dest[^6] = ToHexChar(siblingIndex >> 20);
            if (width >= 5) dest[^5] = ToHexChar(siblingIndex >> 16);
            if (width >= 4) dest[^4] = ToHexChar(siblingIndex >> 12);
            if (width >= 3) dest[^3] = ToHexChar(siblingIndex >> 8);
            if (width >= 2) dest[^2] = ToHexChar(siblingIndex >> 4);
            if (width >= 1) dest[^1] = ToHexChar(siblingIndex);
            
            return buffer;
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
        <= 0x10 << 0 => 1,
        <= 0x10 << 4 => 2,
        <= 0x10 << 8 => 3,
        <= 0x10 << 12 => 4,
        <= 0x10 << 16 => 5,
        <= 0x10 << 20 => 6,
        <= 0x10 << 24 => 7,
        _ => throw new Exception($"Too many siblings: {count}")
    };
}