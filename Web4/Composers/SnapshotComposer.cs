using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public class SnapshotComposer : BaseComposer
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int highWaterMark = 2048;
    
    private StableKeyTreeWalker keyGenerator = new();

    public Keyhole[] Snapshot { get; private set; } = [];

    public Keyhole[] CreateSnapshotAndClear(Func<Html> html)
    {
        Snapshot = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
        return CreateSnapshotAndClear($"{html()}");
    }

    private Keyhole[] CreateSnapshotAndClear([InterpolatedStringHandlerArgument("")] Html html)
    {
        var result = Snapshot;
        Snapshot = [];
        return result;
    }

    protected override void Clear()
    {
        // The first keyhole uses its Integer property to denote the 
        // full buffer length, not just the root-level Html length.
        Snapshot[0].Integer = keyGenerator.WriteHead;

        base.Clear();
    }

    public override void OnHtmlPartialBegins(ref Html html)
    {
        if (IsInitialHtml())
        {
            html.Index = -1;
        }
        else if (IsBeforeAppend())
        {
            html.Key = string.Empty;
            html.Index = 0;
            Snapshot[0].Length = html.Length;
            keyGenerator.Reset();
            keyGenerator.CreateNewGeneration(string.Empty, html.Length);
        }
        else
        {
            var key = keyGenerator.GetNextKey();
            html.Key = key;
            html.Index = keyGenerator.WriteHead;
            keyGenerator.CreateNewGeneration(key, html.Length);
        }

        base.OnHtmlPartialBegins(ref html);
    }

    public override bool OnHtmlPartialEnds(ref Html parent, ref Html partial, string? format = null, string? expression = null)
    {
        // By this point, the `Html partial` has already set its keyholes.
        // They're just later in the buffer, starting at the "high water mark."

        if (parent.Index >= 0)
        {
            // Since the partial has been written, 
            // return to where we left off (a little like recursion).
            // so that we can set the partial's type, expression, key, and range.
            var index = parent.Index + parent.Cursor;
            ref var keyhole = ref Snapshot[index];
            keyhole.Key = partial.Key;
            keyhole.Type = FormatType.Html;
            keyhole.String = expression;
            keyhole.Integer = partial.Index;
            keyhole.Length = partial.Length;

            keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        }

        return base.OnHtmlPartialEnds(ref parent, ref partial, format, expression);
    }

    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        if (parent.Index >= 0)
        {
            var index = parent.Index + parent.Cursor;
            ref var keyhole = ref Snapshot[index];
            keyhole.String = literal;
            keyhole.Type = FormatType.StringLiteral;
        }

        return base.WriteImmutableMarkup(ref parent, literal);
    }

    public override bool WriteMutableValue(ref Html parent, string value)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.String = value;
        keyhole.Type = FormatType.String;
        keyhole.Format = null;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, bool value)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Boolean = value;
        keyhole.Type = FormatType.Boolean;
        keyhole.Format = null;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Color = value;
        keyhole.Type = FormatType.Color;
        keyhole.Format = format;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Uri = value;
        keyhole.Type = FormatType.Uri;
        keyhole.Format = format;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null)
    // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Format = format;

        switch (value)
        {
            case int i:
                keyhole.Integer = i;
                keyhole.Type = FormatType.Integer;
                break;
            case long l:
                keyhole.Long = l;
                keyhole.Type = FormatType.Long;
                break;
            case float f:
                keyhole.Float = f;
                keyhole.Type = FormatType.Float;
                break;
            case double d:
                keyhole.Double = d;
                keyhole.Type = FormatType.Double;
                break;
            case decimal m:
                keyhole.Decimal = m;
                keyhole.Type = FormatType.Decimal;
                break;
            case DateTime dt:
                keyhole.DateTime = dt;
                keyhole.Type = FormatType.DateTime;
                break;
            case DateOnly dO:
                keyhole.DateOnly = dO;
                keyhole.Type = FormatType.DateOnly;
                break;
            case TimeSpan ts:
                keyhole.TimeSpan = ts;
                keyhole.Type = FormatType.TimeSpan;
                break;
            case TimeOnly tO:
                keyhole.TimeOnly = tO;
                keyhole.Type = FormatType.TimeOnly;
                break;
            default:
                // In the future, possibly support other/custom IUtf8SpanFormattable types?
                // This may require much boxing/unboxing.
                // Currently Html.cs is a gatekeeper for these types.
                // So this is currently technically a dead code path.
                throw new NotSupportedException($"Type {typeof(T)} not supported");
        }

        return base.WriteMutableValue(ref parent, value, format);
    }

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    private bool WriteEventListener(ref Html parent, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.EventListener;
        keyhole.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        // TODO: Under the hood this calls `IEnumerable<T>.Count<T>()`.  If it does not
        // also implement ICollection then it will iterate in order to find the count
        // which will instantiate Html values too early thus breaking all the things.
        var itemCount = partials.Count;

        // Reserve a keyhole to represent the loop itself
        var key = keyGenerator.GetNextKey();
        ref var enumerableKeyhole = ref Snapshot[parent.Index + parent.Cursor];
        enumerableKeyhole.Key = key;
        enumerableKeyhole.Type = FormatType.Enumerable;
        enumerableKeyhole.String = expression;
        enumerableKeyhole.Integer = keyGenerator.WriteHead;
        enumerableKeyhole.Length = itemCount;

        int i = 0, index = keyGenerator.WriteHead;
        keyGenerator.CreateNewGeneration(key, itemCount);

        // Note: foreach calls `enumerator.Current` which creates new `Html`s which 
        // triggers `OnHtmlPartialBegins` and `OnHtmlPartialEnds` (above) to be called.
        foreach (var partial in partials)
        {
            keyGenerator.ReturnToParent(key, i * 2 - 1, itemCount);

            ref var itemKeyhole = ref Snapshot[index + i];
            itemKeyhole.Key = keyGenerator.GetNextKey();
            itemKeyhole.Type = FormatType.Html;
            itemKeyhole.String = expression;
            itemKeyhole.Integer = partial.Index;
            itemKeyhole.Length = partial.Length;

            i++;
        }

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }
}

public static class SnapshotComposerExtension
{
    [ThreadStatic]
    static SnapshotComposer? current;

    public static Keyhole[] CreateSnapshot(this Func<Html> html)
    {
        current ??= new SnapshotComposer();
        return current.CreateSnapshotAndClear(html);
    }

    public static void Return(this Keyhole[] buffer)
    {
        ArrayPool<Keyhole>.Shared.Return(buffer);
    }
}