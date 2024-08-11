using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Xui.Web.Html;

public partial struct HtmlString
{
    readonly Composition composition;

    public IEnumerable<Delta> GetDeltas(HtmlString compare)
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
            var chunk = compare.composition.chunks[range.Start.Value];
            if (range.Start.Value == range.End.Value)
            {
                // Not a range, just a single value.  Mutate with nodeValue-precision.
                var output = new StringBuilder();
                chunk.Append(output);
                yield return new Delta(
                    Id: chunk.Id,
                    Type: DeltaType.NodeValue, // TODO: Support attribute!
                    Output: output
                );
            }
            else
            {
                // This is a range of changes.  Replace the whole HTML partial.
                var output = new StringBuilder();
                compare.ToStringWithExtras(range.Start.Value, range.End.Value - 1, output);
                yield return new Delta(
                    Id: chunk.Id,
                    Type: DeltaType.HtmlPartial,
                    Output: output
                );
            }
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

    internal ValueTask<FlushResult> WriteAsync(PipeWriter writer, CancellationToken cancellationToken = default)
    {
        bool hackProbablyAnAttributeNext = false;

        for (int i = start; i < end; i++)
        {
            var chunk = composition.chunks[i];

            switch (chunk.Type)
            {
                case FormatType.Boolean:
                case FormatType.DateTime:
                case FormatType.Integer:
                case FormatType.String:
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
                    writer.Write(ref chunk);
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

        return writer.FlushAsync();
    }

    internal void ToStringWithExtras(int start, int end, StringBuilder builder)
    {
        bool hackProbablyAnAttributeNext = false;

        for (int i = start; i <= end; i++)
        {
            var chunk = composition.chunks[i];

            switch (chunk.Type)
            {
                case FormatType.Boolean:
                case FormatType.DateTime:
                case FormatType.Integer:
                case FormatType.String:
                    if (hackProbablyAnAttributeNext)
                    {
                        chunk.Append(builder);
                    }
                    else
                    {
                        // TODO: After "attribute support" is baked in, this block needs to move back to... Context.cs?
                        builder.Append("<!-- -->");
                        chunk.Append(builder);
                        builder.Append("<script>r(\"slot");
                        builder.Append(chunk.Id);
                        builder.Append("\")</script>");
                    }
                    break;
                case FormatType.View:
                case FormatType.HtmlString:
                    // Only render extras for HtmlString's trailing sentinel, ignore for the leading sentinel.
                    if (chunk.Id > chunk.Integer)
                    {
                        builder.Append("<script>r(\"slot");
                        builder.Append(chunk.Id);
                        builder.Append("\")</script>");
                    }
                    // else
                    // {
                    //     builder.AppendLine();
                    // }

                    break;
                case FormatType.Action:
                case FormatType.ActionAsync:
                    builder.Append("h(");
                    builder.Append(chunk.Id);
                    builder.Append(")");
                    break;
                case FormatType.ActionEvent:
                case FormatType.ActionEventAsync:
                    builder.Append("h(");
                    builder.Append(chunk.Id);
                    builder.Append(",event)");
                    break;
                default:
                    chunk.Append(builder);
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