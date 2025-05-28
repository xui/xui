using System.Buffers;

namespace Web4;

public static class Snapshot
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int highWaterMark = 2048;
    
    public static Keyhole[] Rent()
    {
        return ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    }

    public static void Return(this Keyhole[] buffer)
    {
        ArrayPool<Keyhole>.Shared.Return(buffer);
    }

    public static IEnumerable<int> Diff(this Keyhole[] before, Keyhole[] after)
    {
        // using var perf = Debug.PerfCheck("Diff"); // TODO: Remove PerfCheck

        var length = after[0].Length;

        for (int i = 0; i < length; i++)
        {
            ref var keyholeBefore = ref before[i];
            ref var keyholeAfter = ref after[i];

            switch (keyholeBefore.Type)
            {
                // TODO: Implement
                case FormatType.Html:
                case FormatType.View:
                case FormatType.Attribute:
                case FormatType.EventListener:
                    continue;
            }

            if (!Keyhole.Equals(ref keyholeBefore, ref keyholeAfter))
            {
                yield return i;
            }
        }
    }
}