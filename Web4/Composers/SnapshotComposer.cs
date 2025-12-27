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

    private StableKeyTreeWalker keyGenerator = new();
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
        base.Reset();
    }

    public override void OnElementBegin(ref Html html)
    {
        if (IsRootTemplate)
        {
            html.Index = -1;
        }
        else if (IsBeforeAppend)
        {
            html.Key = string.Empty;
            html.Index = 0;
            snapshot[0].SequenceLength = html.Length;
            keyGenerator.Reset();
            keyGenerator.CreateNewGeneration(string.Empty, html.Length);
        }
        else
        {
            var key = keyGenerator.GetNextKey();
            html.Key = key;
            html.Index = keyGenerator.WriteHead;
            html.IsAttribute = isWritingAttribute;
            keyGenerator.CreateNewGeneration(key, html.Length);
        }

        base.OnElementBegin(ref html);
    }

    public override bool OnElementEnd(ref Html parent, scoped Html partial, string? format = null, string? expression = null)
    {
        // By this point, the `Html partial` has already set its keyholes.
        // They're just later in the buffer, starting at the "high water mark."

        if (parent.Index >= 0)
        {
            // Since the partial has been written, 
            // return to where we left off (a little like recursion).
            // so that we can set the partial's type, expression, key, and range.
            var index = parent.Index + parent.Cursor;
            ref var keyhole = ref snapshot[index];
            keyhole.Key = partial.Key;
            keyhole.Type = partial.IsAttribute ? KeyholeType.Attribute : KeyholeType.Html;
            keyhole.Format = format;
            keyhole.Expression = expression;
            keyhole.SequenceStart = partial.Index;
            keyhole.SequenceLength = partial.Length;
            keyhole.RelativeOrder = partial.RelativeOrder;

            keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        }

        return base.OnElementEnd(ref parent, partial, format, expression);
    }

    public override bool OnMarkup(ref Html parent, string literal)
    {
        if (parent.Index >= 0)
        {
            var index = parent.Index + parent.Cursor;
            ref var keyhole = ref snapshot[index];
            keyhole.String = literal;
            keyhole.Type = KeyholeType.StringLiteral;
            keyhole.SequenceStart = index;
            keyhole.SequenceLength = parent.Length;
            isWritingAttribute = literal.EndsWith('=');
        }

        return base.OnMarkup(ref parent, literal);
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
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref snapshot[index];
        keyhole.SetValue(value);
        keyhole.Type = type;
        keyhole.Format = format;
        if (parent.IsAttribute)
        {
            keyhole.Key = parent.Key; // use parent's key, no need for its own
            keyhole.ParentStart = parent.Index;
        }
        else
        {
            keyhole.Key = keyGenerator.GetNextKey();
            keyhole.IsValueAnAttribute = isWritingAttribute;
        }

        return CompleteFormattedValue();
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnListener(ref parent, format, expression);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnListener(ref parent, format, expression);
    private bool OnListener(ref Html parent, string? format = null, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = KeyholeType.EventListener;
        keyhole.Format = format;
        keyhole.Expression = expression;

        return CompleteFormattedValue();
    }

    public override bool OnIterate<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        var itemCount = partials.Count;

        // Reserve a keyhole to represent the loop itself
        var key = keyGenerator.GetNextKey();
        ref var enumerableKeyhole = ref snapshot[parent.Index + parent.Cursor];
        enumerableKeyhole.Key = key;
        enumerableKeyhole.Type = KeyholeType.Enumerable;
        enumerableKeyhole.Format = format;
        enumerableKeyhole.Expression = expression;
        enumerableKeyhole.SequenceStart = keyGenerator.WriteHead;
        enumerableKeyhole.SequenceLength = itemCount;

        int i = 0, index = keyGenerator.WriteHead;
        keyGenerator.CreateNewGeneration(key, itemCount);

        // Note: foreach calls `enumerator.Current` which creates new `Html`s which 
        // triggers `OnElementBegin` to be called.
        var enumerator = partials.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var (selector, item) = enumerator.CurrentDeconstructed;
            var partial = selector(item);


            ref var itemKeyhole = ref snapshot[index + i];
            itemKeyhole.Key = partial.Key;
            itemKeyhole.Type = KeyholeType.Html;
            itemKeyhole.Format = format;
            itemKeyhole.SequenceStart = partial.Index;
            itemKeyhole.SequenceLength = partial.Length;
            itemKeyhole.RelativeOrder = partial.RelativeOrder;
            itemKeyhole.Tag = item; // TODO: Memory allocation?

            keyGenerator.ReturnToParent(key, ++i * 2 - 1, itemCount);
        }

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }
}