using System.Runtime.CompilerServices;
using Web4.Composers;

namespace Web4;

public static class HotSpot
{
    private readonly static Dictionary<string, Stats> stats = [];

    private class Stats
    {
        public int LiteralLength { get; set; } = 0;
        public int FormattedCount { get; set; } = 0;
    }

    public static void Track(string pattern, IComposer composer)
    {
        if (!stats.TryGetValue(pattern, out var stat))
        {
            stat = new Stats
            {
                // LiteralLength = composer.LiteralLength,
                // FormattedCount = composer.FormattedCount,
            };
            stats[pattern] = stat;
        }
    }

    public static int? GetContentLengthIfConst(string pattern)
    {
        if (stats.TryGetValue(pattern, out var stat))
        {
            if (stat.FormattedCount == 1)
            {
                return stat.LiteralLength;
            }
        }
        return null;
    }

    public static bool ShouldBuffer(string pattern)
    {
        // TODO: There's another potential optimization when writes are small and many 
        // (which can be calculated by the ratio of LiteralLength to FormattedCount),
        // by avoiding the overhead of so many GetSpan calls by writing to a single,
        // pre-allocated buffer instead.
        return false;
    }
}