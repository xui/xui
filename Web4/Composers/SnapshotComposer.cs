using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public class SnapshotComposer : BaseComposer
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int highWaterMark = 2048;
    [ThreadStatic] static SnapshotComposer? reusable;
    public static SnapshotComposer Shared => reusable ??= new SnapshotComposer();

    private readonly KeyCursor keyCursor = new();
    private int writeHead = 0;
    private bool isWritingAttribute = false;
    private Keyhole[] snapshot = [];

    public Keyhole[] Capture(Func<Html> template)
    {
        snapshot = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
        return Interpolate($"{template()}");
    }
    
    public Keyhole[] Capture(Keyhole[] buffer, Func<Html> template)
    {
        snapshot = buffer;
        return Interpolate($"{template()}");
    }

    private Keyhole[] Interpolate([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the templating work has already completed.
        
        // Hang onto the result before html.Dispose() resets this class.
        var result = snapshot;

        // html.Dispose() calls composer.Reset() which sets snapshot to [].
        html.Dispose();

        // Do something interesting with the result.
        return result;
    }
    
    public override void Reset()
    {
        snapshot = [];
        writeHead = 0;
        keyCursor.Reset();
        base.Reset();
    }

    public override bool OnTemplateBegin(ref Html html, ref string literal)
    {
        html.Start = writeHead;
        writeHead += html.Length;
        return true;
    }

    public override bool OnElementBegin(ref Html html)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current; // TODO: Remove?
        keyCursor.MoveDown();
        html.Type = isWritingAttribute ? HtmlType.Attribute : HtmlType.Element;
        html.Start = writeHead;
        writeHead += html.Length;
        return true;
    }

    public override bool OnElementEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        // By this point, the `Html html` parameter has already set its keyholes.
        // They're just later in the buffer, starting at the "high water mark."

        keyCursor.MoveUp();
        var key = keyCursor.Current;

        // Since the html has been written, 
        // return to where we left off (a little like recursion).
        // so that we can set the html's type, expression, key, and range.
        var index = parent.Start + parent.Cursor;
        ref var keyhole = ref snapshot[index];
        keyhole.Key = key;
        keyhole.Type = html.Type switch {
            HtmlType.Attribute => KeyholeType.Attribute,
            HtmlType.Iterator => KeyholeType.Iterator,
            HtmlType.Element or _ => KeyholeType.Html,
        };
        keyhole.Format = format;
        keyhole.Expression = expression;
        keyhole.SequenceStart = html.Start;
        keyhole.SequenceLength = html.Length;
        keyhole.RelativeOrder = html.RelativeOrder;
        return true;
    }

    public override bool OnMarkup(ref Html parent, string literal)
    {
        var index = parent.Start + parent.Cursor;
        ref var keyhole = ref snapshot[index];
        keyhole.String = literal;
        keyhole.Type = KeyholeType.StringLiteral;
        keyhole.SequenceStart = index;
        keyhole.SequenceLength = parent.Length;
        isWritingAttribute = literal.EndsWith('=');
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
        keyCursor.MoveNext();
        var key = keyCursor.Current;

        var index = parent.Start + parent.Cursor;
        ref var keyhole = ref snapshot[index];
        keyhole.SetValue(value);
        keyhole.Type = type;
        keyhole.Format = format;
        if (parent.Type == HtmlType.Attribute)
        {
            keyhole.Key = keyCursor.Parent; // use parent's key, no need for its own
            keyhole.ParentStart = parent.Start;
        }
        else
        {
            keyhole.Key = key;
            keyhole.IsValueAnAttribute = isWritingAttribute;
        }
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnListener(ref parent, format, expression);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, format, expression);
    private bool OnListener(ref Html parent, string? format = null, string? expression = null)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current;

        var index = parent.Start + parent.Cursor;
        ref var keyhole = ref snapshot[index];
        keyhole.Key = key;
        keyhole.Type = KeyholeType.EventListener;
        keyhole.Format = format;
        keyhole.Expression = expression;
        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current;
        keyCursor.MoveDown();

        htmls.Start = writeHead;

        ref var keyhole = ref snapshot[parent.Start + parent.Cursor];
        keyhole.Key = key;
        keyhole.Type = KeyholeType.Iterator;
        keyhole.Format = format;
        keyhole.Expression = expression;
        keyhole.SequenceStart = htmls.Start;
        keyhole.SequenceLength = htmls.Length;

        writeHead += htmls.Length;
        return true;
    }

    public override bool OnIterate<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var (selector, item) = enumerator.CurrentDeconstructed;
            var html = selector(item);

            htmls.AppendFormatted(html);

            ref var keyhole = ref snapshot[htmls.Start + htmls.Cursor - 1];
            keyhole.Tag = item; // TODO: Memory allocation?
        }
        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        keyCursor.MoveUp();
        return true;
    }
}