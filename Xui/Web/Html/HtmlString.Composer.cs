using System.Buffers;
using System.Text;

namespace Xui.Web;

public partial struct Html
{
    public readonly Span<Chunk> AsSpan()
    {
        return composer.chunks.AsSpan(start, end - start);
    }

    public IEnumerable<Memory<Chunk>> GetDeltas(Html compare)
    {
        List<Range>? ranges = null;
        for (int index = 0; index < end; index++)
        {
            var oldChunk = composer.chunks[index];
            var newChunk = compare.composer.chunks[index];
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
                var htmlStringStart = compare.composer.chunks[newChunk.Integer!.Value];
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
                array: compare.composer.chunks, 
                start: start, 
                length: end - start + 1
            );
        }
    }

    public void HandleEvent(int slotId, Event? domEvent)
    {
        // TODO: These should not block the Context.Receive event loop.
        // So none of these will be awaiting.  But that could cause some 
        // tricky overlapping.  I bet the user is expecting them to execute
        // in order?  Do I need a queue?  But this queue should belong to the Context?

        // TODO: Optimize.  Bypass the O(n).  Lazy Dict gets reset on each compose?
        var chunk = composer.chunks.First(c => c.Id == slotId);
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
        for (int i = 1; i < end; i++)
        {
            ref var chunk = ref composer.chunks[i];

            if (chunk.Format != null)
                return null;

            if (chunk.Type != FormatType.StringLiteral)
                return null;

            contentLength += chunk.String!.Length;
        }
        return contentLength;
    }
}