using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Xui.Web.Html;

public partial struct HtmlString
{
    readonly Composition composition;

    internal IEnumerable<Memory<Chunk>> GetDeltas(HtmlString compare)
    {
        List<Range>? ranges = null;
        for (int index = 0; index < end; index++)
        {
            var oldChunk = composition.chunks[index];
            var newChunk = compare.composition.chunks[index];
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
                var htmlStringStart = compare.composition.chunks[newChunk.Integer!.Value];
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
                array: compare.composition.chunks, 
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
        var chunk = composition.chunks.First(c => c.Id == slotId);
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
            ref var chunk = ref composition.chunks[i];

            if (chunk.Format != null)
                return null;

            if (chunk.Type != FormatType.StringLiteral)
                return null;

            contentLength += chunk.String!.Length;
        }
        return contentLength;
    }

    internal readonly void Write(PipeWriter writer)
    {
        bool hackProbablyAnAttributeNext = false;

        for (int i = start; i < end; i++)
        {
            ref var chunk = ref composition.chunks[i];

            switch (chunk.Type)
            {
                case FormatType.View:
                case FormatType.HtmlString:
                    // Only render extras for HtmlString's trailing sentinel, ignore for the leading sentinel.
                    if (chunk.Id > chunk.Integer)
                    {
                        writer.WriteStringLiteral("<script>r(\"slot");
                        writer.Write(chunk.Id);
                        writer.WriteStringLiteral("\")</script>");
                    }
                    break;
                case FormatType.Action:
                case FormatType.ActionAsync:
                    writer.WriteStringLiteral("h(");
                    writer.Write(chunk.Id);
                    writer.WriteStringLiteral(")");
                    break;
                case FormatType.ActionEvent:
                case FormatType.ActionEventAsync:
                    writer.WriteStringLiteral("h(");
                    writer.Write(chunk.Id);
                    writer.WriteStringLiteral(",event)");
                    break;
                default:
                    if (hackProbablyAnAttributeNext)
                    {
                        writer.Write(ref chunk);
                    }
                    else
                    {
                        writer.WriteStringLiteral("<!-- -->");
                        writer.Write(ref chunk);
                        writer.WriteStringLiteral("<script>r(\"slot");
                        writer.Write(chunk.Id);
                        writer.WriteStringLiteral("\")</script>");
                    }
                    break;
            }

            if (chunk.Type == FormatType.StringLiteral && chunk.String?[^1] == '"')
            {
                hackProbablyAnAttributeNext = true;
            }
            else
            {
                hackProbablyAnAttributeNext = false;
            }
        }
    }
}