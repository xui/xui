using System.Runtime.CompilerServices;
using System.Text;

namespace Web4;

public class KeyCursor
{
    private const byte separator = (byte)'_';
    private int currentLevel;
    private readonly List<int> levels = [-1];
    private KeyCache keyCache = KeyCache.Root;

    public string Parent => Encoding.UTF8.GetString(keyCache.Key); // TODO: string -> byte[]

    public KeyCursor() => Reset();

    public void Reset()
    {
        currentLevel = 0;
        levels[0] = -1;
        keyCache = KeyCache.Root;
    }

    public void MoveNext() => levels[currentLevel]++;

    public void MoveDown()
    {
        keyCache = keyCache.NextGeneration(levels[currentLevel]);
        currentLevel++;

        if (levels.Count > currentLevel)
            levels[currentLevel] = -1;
        else
            levels.Add(-1);
    }

    public void MoveUp()
    {
        keyCache = keyCache.Parent;
        currentLevel--;
    }

    public string Current => Encoding.UTF8.GetString(CurrentAsBytes);
    public byte[] CurrentAsBytes
    {
        get
        {
            int index = levels[currentLevel];

            var key = keyCache[index];
            if (key is not null)
                return key;

            key = GenerateKey(
                parentKey: keyCache.Key, 
                childIndex: index
            );

            keyCache[index] = key;

            return key;
        }
    }

    private static byte[] GenerateKey(byte[] parentKey, int childIndex)
    {
        int length = GetLength(childIndex);
        var buffer = new byte[parentKey.Length + length + 1];
        parentKey.CopyTo(buffer);
        buffer[parentKey.Length] = separator;

        Span<byte> dest = buffer.AsSpan(parentKey.Length + 1);
        if (length >= 7) dest[^7] = ToHexChar(childIndex >> 24);
        if (length >= 6) dest[^6] = ToHexChar(childIndex >> 20);
        if (length >= 5) dest[^5] = ToHexChar(childIndex >> 16);
        if (length >= 4) dest[^4] = ToHexChar(childIndex >> 12);
        if (length >= 3) dest[^3] = ToHexChar(childIndex >> 8);
        if (length >= 2) dest[^2] = ToHexChar(childIndex >> 4);
        if (length >= 1) dest[^1] = ToHexChar(childIndex);
        return buffer;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLength(int number) => number switch
    {
        < 0x10 << 0 => 1,  // 0-f
        < 0x10 << 4 => 2,  // 10-ff
        < 0x10 << 8 => 3,  // 100-fff
        < 0x10 << 12 => 4, // 1000-ffff
        < 0x10 << 16 => 5, // 10000-fffff
        < 0x10 << 20 => 6, // 100000-fffff
        < 0x10 << 24 => 7, // 1000000-fffffff
        _ => throw new Exception($"Too large for an int: {number}")
    };
}