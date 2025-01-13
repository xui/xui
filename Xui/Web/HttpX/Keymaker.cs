namespace Xui.Web.HttpX;

internal struct Keymaker
{
    // Note: Not all characters are used so that no curse words can be injected into people's HTML.
    private static readonly char[] VALID_CHARS = "abcdef0123456789".ToCharArray();
    private static readonly int BASE = VALID_CHARS!.Length;
    private static Dictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> cache 
        = new Dictionary<string, string>().GetAlternateLookup<ReadOnlySpan<char>>();

    /// <summary>
    /// Creates a key that can consistently identify a keyhole in the slot table.
    /// Caches keys to avoid rampant memory allocations.
    /// </summary>
    /// <returns></returns>
    public static string GetKey(string parentKey, int cursor, int siblings)
    {
        var numberWidth = GetNumberWidth(siblings / 2);
        var keyLength = parentKey.Length + numberWidth;

        Span<char> key = stackalloc char[keyLength];
        parentKey.CopyTo(key);

        while (numberWidth > 0)
        {
            var index = parentKey.Length + numberWidth - 1;
            var thisDigit = cursor % BASE;
            key[index] = VALID_CHARS[thisDigit];
            cursor /= BASE;
            numberWidth--;
        }

        if (!cache.TryGetValue(key, out var value))
        {
            value = new string(key);
            cache[value] = value;
            return value;
        }
        return value;
    }

    /// <summary>
    /// Converts ReadOnlySpan<byte> to string from a pool of keys
    /// to prevent rampant memory allocations.
    /// Prevents OutOfMemory issues by only returning a key 
    /// if its already in the pool due to hackers filling the pool 
    /// with nonsensical keys from custom-constructed GET/SET/CALL requests.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal static string? GetKeyIfCached(ReadOnlySpan<byte> key)
    {
        if (key.Length > 1024)
            return null;
        
        Span<char> chars = stackalloc char[key.Length];
        for (int i = 0; i < key.Length; i++)
            chars[i] = (char)key[i];

        if (cache.TryGetValue(chars, out var value))
            return value;
        
        return null;
    }

    private static int GetNumberWidth(int digit)
    {
        var width = 0;
        while (true)
            if (Math.Pow(BASE, ++width) >= digit)
                return width;
    }
}