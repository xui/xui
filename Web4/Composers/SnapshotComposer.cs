using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public static class SnapshotComposerExtension
{
    [ThreadStatic]
    static SnapshotComposer? current;

    public static Keyhole[] CreateSnapshot(this Func<Html> html)
    {
        current ??= new SnapshotComposer();
        return current.CreateSnapshotAndClear(html);
    }
}

public class SnapshotComposer : BaseComposer
{
    private StableKeyTreeWalker keyGenerator = new();

    public Keyhole[] Snapshot { get; private set; } = [];

    public Keyhole[] CreateSnapshotAndClear(Func<Html> html)
    {
        Snapshot = Web4.Snapshot.Rent();
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
        keyGenerator.Reset();

        // The first keyhole uses its Integer property to denote the 
        // full buffer length, not just the root-level Html length.
        Snapshot[0].Integer = keyGenerator.WriteHead;

        base.Clear();
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

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
    // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // Must trigger Html to append its splits.  Then reset the parent.
        _ = attrValue(null!);

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Color> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Uri> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => WriteEventListener(ref parent, expression);
    public override bool WriteEventListener(ref Html parent, ReadOnlySpan<char> argName, Action<object> listener, string? expression = null) => WriteEventListener(ref parent, expression);
    private bool WriteEventListener(ref Html parent, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.EventListener;
        keyhole.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TComponent>(ref Html parent, ref TComponent component, string? format = null, string? expression = null)
    {
        return WriteMutableElement(ref parent, component.Render(), format, expression);
    }

    public override void OnPartialBegins(ref Html html)
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

        base.OnPartialBegins(ref html);
    }

    public override bool WriteMutableElement(ref Html parent, Html partial, string? format = null, string? expression = null)
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

        return base.WriteMutableElement(ref parent, partial, format, expression);
    }

    public override bool WriteMutableElement<T>(ref Html parent, HtmlEnumerable<T> partials, string? format = null, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot[index];
        keyhole.Key = keyGenerator.GetNextKey();
        keyhole.Type = FormatType.Enumerable;
        keyhole.String = expression;

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);

        return base.WriteMutableElement(ref parent, partials, format, expression);
    }

    // public IEnumerable<Keyhole> EnumerateDepthFirst(Keyhole keyhole)
    // {
    //     var start = keyhole.Integer;
    //     var end = start + keyhole.Long - 1;
    //     for (int i = start; i <= end; i++)
    //     {
    //         var k = Snapshot[i];
    //         yield return k;

    //         if (k.Type == FormatType.Html)
    //         {
    //             foreach (var item in EnumerateDepthFirst(k))
    //             {
    //                 yield return item;
    //             }
    //         }
    //     }
    // }
}