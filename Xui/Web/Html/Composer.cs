namespace Xui.Web;

public class Composer
{
    [ThreadStatic]
    static Composer? current;
    public static Composer? Current { get => current; set => current = value; }

    internal Chunk[] chunks = new Chunk[1000];
    internal int cursor = 0;
    internal int depth = 0;

    // TODO: Remove tempEnd!
    public int tempEnd = 0;

    public Span<Chunk> AsSpan()
    {
        return chunks.AsSpan(0, tempEnd);
    }

    public void HandleEvent(int slotId, Event? domEvent)
    {
        // TODO: These should not block the Context.Receive event loop.
        // So none of these will be awaiting.  But that could cause some 
        // tricky overlapping.  I bet the user is expecting them to execute
        // in order?  Do I need a queue?  But this queue should belong to the Context?

        // TODO: Optimize.  Bypass the O(n).  Lazy Dict gets reset on each compose?
        var chunk = chunks.First(c => c.Id == slotId);
        switch (chunk.Type)
        {
            case FormatType.Action:
                chunk.Action();
                break;
            case FormatType.ActionEvent:
                chunk.ActionEvent(domEvent ?? Event.Empty);
                break;
            case FormatType.ActionAsync:
                // Do not batch.  Mutations should go immediately.
                // Do not await. That'd block this event loop.
                _ = chunk.ActionAsync();
                break;
            case FormatType.ActionEventAsync:
                // Do not batch.  Mutations should go immediately.
                // Do not await. That'd block this event loop.
                _ = chunk.ActionEventAsync(domEvent ?? Event.Empty);
                break;
        }
    }

    public int? GetContentLengthIfConvenient()
    {
        // Only return a number if it doesn't involve a bunch of extra work 
        // like parsing/applying a formatter.

        int contentLength = 0;
        for (int i = 1; i < tempEnd; i++)
        {
            ref var chunk = ref chunks[i];

            if (chunk.Format != null)
                return null;

            if (chunk.Type != FormatType.StringLiteral)
                return null;

            contentLength += chunk.String!.Length;
        }
        return contentLength;
    }

    public IEnumerable<Memory<Chunk>> GetDeltas(Composer compare)
    {
        List<Range>? ranges = null;
        for (int index = 0; index < tempEnd; index++)
        {
            var oldChunk = chunks[index];
            var newChunk = compare.chunks[index];
            if (oldChunk == newChunk) {
                continue;
            }

            ranges ??= [];

            if (newChunk.Type != FormatType.StringLiteral)
            {
                ranges.Add(new Range(newChunk.Id, newChunk.Id));
            }
            else
            {
                var htmlStringStart = compare.chunks[newChunk.Integer!.Value];
                ranges.Add(new Range(newChunk.Integer.Value, htmlStringStart.Integer!.Value));
            }
        }

        if (ranges == null)
            yield break;

        ranges.Sort((a, b) => a.Start.Value - b.Start.Value);
        int i = 0, max = -1;
        while (i < ranges.Count)
        {
            var range = ranges[i];
            if (max >= range.Start.Value)
                ranges.RemoveAt(i);
            else
                i++;
            max = range.End.Value;
        }

        foreach (var range in ranges)
        {
            var start = range.Start.Value;
            var end = range.End.Value;
            yield return new(
                array: compare.chunks, 
                start: start, 
                length: end - start + 1
            );
        }
    }

    public IDisposable ReuseBuffer()
    {
        return new Reuseable(this);
    }

    private class Reuseable : IDisposable
    {
        public Reuseable(Composer composer)
        {
            Composer.Current = composer;
        }

        public void Dispose()
        {
            Composer.Current = null;
        }
    }
}