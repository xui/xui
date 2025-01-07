
namespace Xui.Web.HttpX;

internal struct Keymaker
{
    // Note: Not all characters are used so that no curse words can be injected into people's HTML.
    private static readonly char[] VALID_CHARS = "ABCDEFabcdef0123456789".ToCharArray();
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

    private static int GetNumberWidth(int digit)
    {
        var width = 0;
        while (true)
            if (Math.Pow(BASE, ++width) >= digit)
                return width;
    }
}