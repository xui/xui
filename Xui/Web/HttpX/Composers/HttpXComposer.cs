using System.Buffers;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class HttpXComposer : BufferWriterComposer
{
    internal Chunk[] chunks = new Chunk[1000];
    internal int cursor = 0;
    // internal int depth = 0;

    public int end = 0;

    public HttpXComposer(IBufferWriter<byte> writer)
        : base(writer)
    {
    }

    public override void AppendLiteral(string s)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.String = s;
        // chunk.Integer = start; // TODO: Must reference start of Html partial?
        chunk.Type = FormatType.StringLiteral;

        base.AppendLiteral(s);
    }

    public override void AppendFormatted(string s)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.String = s;
        chunk.Type = FormatType.String;
        chunk.Format = null;

        base.AppendFormatted(s);
    }

    public override void AppendFormatted(int i, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Integer = i;
        chunk.Type = FormatType.Integer;
        chunk.Format = format;

        base.AppendFormatted(i, format);
    }

    public override void AppendFormatted(long l, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Long = l;
        chunk.Type = FormatType.Long;
        chunk.Format = format;

        base.AppendFormatted(l, format);
    }

    public override void AppendFormatted(float f, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Float = f;
        chunk.Type = FormatType.Float;
        chunk.Format = format;

        base.AppendFormatted(f, format);
    }

    public override void AppendFormatted(double d, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Double = d;
        chunk.Type = FormatType.Double;
        chunk.Format = format;

        base.AppendFormatted(d, format);
    }

    public override void AppendFormatted(decimal d, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Decimal = d;
        chunk.Type = FormatType.Decimal;
        chunk.Format = format;

        base.AppendFormatted(d, format);
    }

    public override void AppendFormatted(DateTime d, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.DateTime = d;
        chunk.Type = FormatType.DateTime;
        chunk.Format = format;

        base.AppendFormatted(d, format);
    }

    public override void AppendFormatted(TimeSpan t, string? format = null)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.TimeSpan = t;
        chunk.Type = FormatType.TimeSpan;
        chunk.Format = format;

        base.AppendFormatted(t, format);
    }

    public override void AppendFormatted(bool b)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Boolean = b;
        chunk.Type = FormatType.Boolean;
        chunk.Format = null;

        base.AppendFormatted(b);
    }

    public override void AppendFormatted<TView>(TView v) => AppendFormatted(v.Render());
    public override void AppendFormatted(Slot s) => AppendFormatted(s());

    public override void AppendFormatted(Html h)
    {
        // end = h.end; // TODO: For the cursor or for the partial boundary?

        ref var chunk = ref chunks[end];
        chunk.Id = end;
        // chunk.Integer = h.start; // TODO: For partial boundaries.
        chunk.Type = FormatType.HtmlString;

        ref var start = ref chunks[end];//h.start]; // For partial boundaries.
        start.Integer = end;

        base.AppendFormatted(h);
    }


    public override void AppendFormatted(Action a)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.Action = a;
        chunk.Type = FormatType.Action;

        base.AppendFormatted(a);
    }

    public override void AppendFormatted(Action<Event> a)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.ActionEvent = a;
        chunk.Type = FormatType.ActionEvent;

        base.AppendFormatted(a);
    }

    public override void AppendFormatted(Func<Task> f)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.ActionAsync = f;
        chunk.Type = FormatType.ActionAsync;

        base.AppendFormatted(f);
    }

    public override void AppendFormatted(Func<Event, Task> f)
    {
        ref var chunk = ref chunks[end];
        chunk.Id = end;
        chunk.ActionEventAsync = f;
        chunk.Type = FormatType.ActionEventAsync;

        base.AppendFormatted(f);
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

    public IEnumerable<Memory<Chunk>> GetDeltas(HttpXComposer compare)
    {
        List<Range>? ranges = null;
        for (int index = 0; index < end; index++)
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
}