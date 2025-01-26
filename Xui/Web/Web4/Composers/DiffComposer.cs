using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Web4.Composers;

namespace Web4.Composers;

public class DiffComposer : BaseComposer
{
    private string parentKey = string.Empty;
    private int parentLength = 0;
    private int cursor = 0;
    private int writeHead = -3; // Offset by -3 to skip the initial $"{html()}
    
    public Snapshot Snapshot { get; init; } = new();

    protected override void Clear()
    {
        parentKey = string.Empty;
        parentLength = 0;
        cursor = 0;

        Snapshot.BufferLength = writeHead;
        writeHead = -3;

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        if (IsInitialAppend())
        {
            html.Key = string.Empty;
            Snapshot.RootLength = html.Length;
        }
        else
        {
            html.Key = Keymaker.GetKey(parentKey, cursor++, parentLength);
        }

        parentKey = html.Key;
        parentLength = html.Length;
        cursor = 0;

        html.Index = writeHead;
        writeHead += html.Length;

        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        var index = parent.Index + parent.Cursor;
        if (index >= 0)
        {
            ref var keyhole = ref Snapshot.Buffer[index];
            keyhole.String = literal;
            keyhole.Type = FormatType.StringLiteral;
        }

        return base.WriteImmutableMarkup(ref parent, literal);
    }

    public override bool WriteMutableValue(ref Html parent, string value)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.String = value;
        keyhole.Type = FormatType.String;
        keyhole.Format = null;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, bool value)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.Boolean = value;
        keyhole.Type = FormatType.Boolean;
        keyhole.Format = null;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
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
            case TimeSpan ts:
                keyhole.TimeSpan = ts;
                keyhole.Type = FormatType.TimeSpan;
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
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // Must trigger Html to append its splits.  Then reset the parent.
        _ = attrValue(null!);
        parentKey = parent.Key;
        parentLength = parent.Length;
        cursor = parent.Cursor / 2 + 1;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteEventHandler(ref Html parent, Action eventHandler, string? format = null, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, Action<Event> eventHandler, string? format = null, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, Func<Task> eventHandler, string? format = null, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, Func<Event, Task> eventHandler, string? format = null, string? expression = null) => WriteEventHandler(ref parent, expression);
    private bool WriteEventHandler(ref Html parent, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        ref var keyhole = ref Snapshot.Buffer[index];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        keyhole.Type = FormatType.EventHandler;
        keyhole.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html parent, TView view) => WriteMutableElement(ref parent, view.Render());
    public override bool WriteMutableElement(ref Html parent, Slot slot) => WriteMutableElement(ref parent, slot());
    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null)
    {
        var index = parent.Index + parent.Cursor;
        if (index >= 0)
        {
            ref var keyhole = ref Snapshot.Buffer[index];
            keyhole.Key = partial.Key;
            parentKey = parent.Key;
            parentLength = parent.Length;
            cursor = parent.Cursor / 2 + 1;
            keyhole.Type = FormatType.Html;
            keyhole.String = expression;
            keyhole.Integer = partial.Index;
            keyhole.Long = partial.Cursor;
        }

        return base.WriteMutableElement(ref parent, partial, expression);
    }

    public IEnumerable<Keyhole> EnumerateDepthFirst(Keyhole keyhole)
    {
        var start = keyhole.Integer!.Value;
        var end = start + keyhole.Long!.Value - 1;
        for (int i = start; i <= end; i++)
        {
            var k = Snapshot.Buffer[i];
            yield return k;
            
            if (k.Type == FormatType.Html)
            {
                foreach (var item in EnumerateDepthFirst(k))
                {
                    yield return item;
                }
            }
        }
    }
}