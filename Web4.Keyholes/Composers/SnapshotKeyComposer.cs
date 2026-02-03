using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using MicroHtml;
using Web4.Dom;

namespace Web4.Keyholes.Composers;

public class SnapshotKeyComposer : BaseKeyComposer
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int highWaterMark = 2048;
    [ThreadStatic] static SnapshotKeyComposer? reusable;
    public static SnapshotKeyComposer Shared => reusable ??= new SnapshotKeyComposer();

    private Keyhole[] buffer = [];
    private bool isWritingAttribute = false;
    private int writeHead = 0;
    private readonly List<int> cursors = [0];
    private int Cursor
    {
        get => cursors[keyCursor.CurrentDepth];
        set
        {
            if (cursors.Count == keyCursor.CurrentDepth)
                cursors.Add(value);
            else
                cursors[keyCursor.CurrentDepth] = value;
        }
    }

    public Keyhole[] Capture(Func<Html> template)
    {
        buffer = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
        return Interpolate($"{template()}");
    }
    
    public Keyhole[] Capture(Keyhole[] buffer, Func<Html> template)
    {
        this.buffer = buffer;
        return Interpolate($"{template()}");
    }

    private Keyhole[] Interpolate([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the templating work has already completed.
        
        // Hang onto the result before html.Dispose() resets this class.
        var result = buffer;

        // html.Dispose() calls composer.Reset() which sets snapshot to [].
        html.Dispose();

        // Do something interesting with the result.
        return result;
    }
    
    public override bool OnTemplateBegin(ref Html html, ref string markup)
    {
        base.OnTemplateBegin(ref html, ref markup);
        Cursor = 0;
        writeHead += html.Length + 1;
        return true;
    }

    public override bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1)
    {
        base.OnMarkup(ref parent, ref literal, relativeOrder);

        int index = Cursor;
        ref var keyhole = ref buffer[index];
        keyhole.String = literal;
        keyhole.Type = KeyholeType.StringLiteral;
        keyhole.SequenceStart = index;
        keyhole.SequenceLength = parent.Length;
        isWritingAttribute = literal.EndsWith('=');
       
        Cursor = index + 1;
        return true;
    }

    public override bool OnStringKeyhole(ref Html parent, string value) => OnKeyhole(ref parent, value, KeyholeType.String);
    public override bool OnBoolKeyhole(ref Html parent, bool value) => OnKeyhole(ref parent, value, KeyholeType.Boolean);
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Integer, format);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Long, format);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Float, format);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null)  => OnKeyhole(ref parent, value, KeyholeType.Double, format);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Decimal, format);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.DateTime, format);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.DateOnly, format);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.TimeSpan, format);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.TimeOnly, format);
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Color, format);
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Uri, format);
    private bool OnKeyhole<T>(ref Html parent, T value, KeyholeType type, string? format = null)
    {
        EnsureOddIndex();
        base.OnKeyhole(ref parent);

        int index = Cursor;
        ref var keyhole = ref buffer[index];
        keyhole.Key = Key;
        keyhole.SetValue(value);
        keyhole.Type = type;
        keyhole.Format = format;
        keyhole.IsValueAnAttribute = isWritingAttribute;

        Cursor = index + 1;
        return true;
    }

    public override bool OnHtmlBegin(ref Html html, int relativeOrder = -1)
    {
        EnsureOddIndex();
        int index = Cursor;
        base.OnHtmlBegin(ref html, relativeOrder);

        ref var keyhole = ref buffer[index];
        keyhole.Key = Key;
        keyhole.SequenceStart = writeHead;
        keyhole.SequenceLength = html.Length;
        keyhole.Type = isWritingAttribute ? KeyholeType.Attribute : KeyholeType.Html;
        keyhole.RelativeOrder = relativeOrder;

        Cursor = writeHead;
        writeHead += html.Length + 1;
        return true;
    }

    public override bool OnHtmlEnd(ref Html parent, scoped Html html, int relativeOrder = -1, string? format = null, string? expression = null)
    {
        // Prevent bleeding
        ref var tail = ref buffer[Cursor];
        tail.String = string.Empty;
        tail.Type = KeyholeType.StringLiteral;

        base.OnHtmlEnd(ref parent, html, relativeOrder, format, expression);

        int index = Cursor;
        ref var keyhole = ref buffer[index];
        keyhole.Format = format;
        keyhole.Expression = expression;
        if (relativeOrder >= 0)
            keyhole.RelativeOrder = relativeOrder;

        Cursor = index + 1;
        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        EnsureOddIndex();
        int index = Cursor;
        base.OnIteratorBegin(ref parent, ref htmls, format, expression);

        ref var keyhole = ref buffer[index];
        keyhole.Key = Key;
        keyhole.Type = KeyholeType.Iterator;
        keyhole.Format = format;
        keyhole.Expression = expression;
        keyhole.SequenceStart = writeHead;
        keyhole.SequenceLength = htmls.Length;

        Cursor = writeHead;
        writeHead += htmls.Length + 1;
        return true;
    }

    public override bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        OnIteratorBegin(ref parent, ref htmls, format, expression);

        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var (selector, item) = enumerator.CurrentDeconstructed;
            var html = selector(item);
            htmls.AppendFormatted(html);

            buffer[Cursor - 1].Tag = item; // TODO: Memory allocation?
        }

        OnIteratorEnd(ref parent, ref htmls, format, expression);
        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        // Prevent bleeding
        ref var keyhole = ref buffer[Cursor];
        keyhole.String = string.Empty;
        keyhole.Type = KeyholeType.StringLiteral;

        base.OnIteratorEnd(ref parent, ref htmls, format, expression);

        Cursor++;
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnListener(ref parent, format, expression);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, format, expression);
    private bool OnListener(ref Html parent, string? format = null, string? expression = null)
    {
        EnsureOddIndex();
        base.OnKeyhole(ref parent);

        int index = Cursor;
        ref var keyhole = ref buffer[index];
        keyhole.Key = Key;
        keyhole.Type = KeyholeType.EventListener;
        keyhole.Format = format;
        keyhole.Expression = expression;
        
        Cursor = index + 1;
        return true;
    }

    public override void Reset()
    {
        buffer = [];
        writeHead = 0;
        cursors[0] = 0;
        base.Reset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureOddIndex()
    {
        int index = Cursor;
        if (index % 2 == 0)
        {
            ref var keyhole = ref buffer[index];
            keyhole.String = string.Empty;
            keyhole.Type = KeyholeType.StringLiteral;        
            Cursor = index + 1;
        }
    }
}