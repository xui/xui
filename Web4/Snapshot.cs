using System.Buffers;

namespace Web4;

public class Snapshot : IDisposable
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int highWaterMark = 2048;
    
    public Keyhole[] Buffer { get; private set; }

    public Snapshot()
    {
        Buffer = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    }

    public IEnumerable<int> Diff(Snapshot compare)
    {
        // using var perf = Debug.PerfCheck("Diff"); // TODO: Remove PerfCheck

        var before = this;
        var after = compare;
        var length = after.Buffer[0].Length;

        for (int i = 0; i < length; i++)
        {
            ref var keyholeBefore = ref before.Buffer[i];
            ref var keyholeAfter = ref after.Buffer[i];

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

    public void Dispose()
    {
        ArrayPool<Keyhole>.Shared.Return(Buffer);
    }
}