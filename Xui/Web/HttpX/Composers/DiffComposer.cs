using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class DiffComposer : BaseComposer
{
    private static int highWaterMark = 2048;
    // private Keyhole[] keyholes;
    private static Keyhole[] keyholes = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    private string parentKey = string.Empty;
    private int cursor = 0;
    public Span<Keyhole> Keyholes { get => keyholes.AsSpan(0, Length); }
    public int WriteHead { get; private set; } = 0;
    public int Length { get; private set; } = 0;

    public DiffComposer()
    {
        // keyholes = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    }

    protected override void Clear()
    {
        parentKey = string.Empty;
        cursor = 0;

        Length = WriteHead;
        WriteHead = 0;

        // currentIndex = 0;
        // parentStartIndex = 0;

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        html.Key = Keymaker.GetKey(parentKey, cursor++);
        parentKey = html.Key;
        cursor = 0;

        html.Index = WriteHead;
        WriteHead += html.Length;

        // ref var keyhole = ref keyholes[index];
        // keyhole.Key = Keymaker.GetOrCreate(ref html);
        // keyhole.OldId = Cursor;
        // // keyhole.RefId = parentStartIndex;
        // keyhole.Type = FormatType.HtmlString;
        // keyhole.String = $" --------start-----------: literalLength: {literalLength}, formattedCount: {formattedCount}";

        // // Update the "starting end cap" to point its end.
        // ref var start = ref keyholes[parentStartIndex];
        // start.Integer = Cursor;

        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteImmutableMarkup(ref Html parent, string literal)
    {
        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        // keyhole.Key = Keymaker.GetOrCreate(ref html);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.String = literal;
        keyhole.Type = FormatType.StringLiteral;

        return base.WriteImmutableMarkup(ref parent, literal);
    }

    public override bool WriteMutableValue(ref Html parent, string value)
    {
        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.String = value;
        keyhole.Type = FormatType.String;
        keyhole.Format = null;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue(ref Html parent, bool value)
    {
        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Boolean = value;
        keyhole.Type = FormatType.Boolean;
        keyhole.Format = null;

        return base.WriteMutableValue(ref parent, value);
    }

    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
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

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        // end = h.end;

        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // end = h.end;

        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        // end = h.end;

        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref parent, attrName, attrValue, expression);
    }

    public override bool WriteEventHandler(ref Html parent, Action eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> argName, Action eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> argName, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> argName, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    public override bool WriteEventHandler(ref Html parent, ReadOnlySpan<char> argName, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref parent, expression);
    private bool WriteEventHandler(ref Html parent, string? expression = null)
    {
        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = Keymaker.GetKey(parentKey, cursor++);
        // keyhole.OldId = Cursor;
        keyhole.Type = FormatType.EventHandler;
        keyhole.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html parent, TView view) => WriteMutableElement(ref parent, view.Render());
    public override bool WriteMutableElement(ref Html parent, Slot slot) => WriteMutableElement(ref parent, slot());
    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null)
    {
        ref var keyhole = ref keyholes[parent.Index + parent.Cursor];
        keyhole.Key = partial.Key;
        parentKey = parent.Key;
        cursor = parent.Cursor / 2 + 1;
        // keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Type = FormatType.Html;
        keyhole.String = expression;
        keyhole.Integer = partial.Index;
        keyhole.Long = partial.Cursor;

        // // Update the "starting end cap" to point its end.
        // ref var start = ref keyholes[parentStartIndex];
        // start.Integer = Cursor;

        return base.WriteMutableElement(ref parent, partial, expression);
    }

    public IEnumerable<Keyhole> EnumerateDepthFirst(Keyhole keyhole)
    {
        var start = keyhole.Integer!.Value;
        var end = start + keyhole.Long!.Value - 1;
        for (int i = start; i <= end; i++)
        {
            var k = keyholes[i];
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