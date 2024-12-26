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
    public Span<Keyhole> Keyholes { get => keyholes.AsSpan(0, Length); }
    public int WriteHead { get; private set; } = 0;
    public int Length { get; private set; } = 0;

    public DiffComposer()
    {
        // keyholes = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    }

    protected override void Clear()
    {
        Length = WriteHead;
        WriteHead = 0;

        // currentIndex = 0;
        // parentStartIndex = 0;

        Keymaker.MoveUp(1);

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        Keymaker.MoveDown(1);

        html.Index = WriteHead;
        WriteHead += html.Length;

        // ref var keyhole = ref keyholes[index];
        // keyhole.Key = Keymaker.GetNext();
        // keyhole.OldId = Cursor;
        // // keyhole.RefId = parentStartIndex;
        // keyhole.Type = FormatType.HtmlString;
        // keyhole.String = $" --------start-----------: literalLength: {literalLength}, formattedCount: {formattedCount}";

        // // Update the "starting end cap" to point its end.
        // ref var start = ref keyholes[parentStartIndex];
        // start.Integer = Cursor;

        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteImmutableMarkup(ref Html html, string literal)
    {
        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        // keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.String = literal;
        keyhole.Type = FormatType.StringLiteral;

        return base.WriteImmutableMarkup(ref html, literal);
    }

    public override bool WriteMutableValue(ref Html html, string value)
    {
        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.String = value;
        keyhole.Type = FormatType.String;
        keyhole.Format = null;

        return base.WriteMutableValue(ref html, value);
    }

    public override bool WriteMutableValue(ref Html html, bool value)
    {
        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Boolean = value;
        keyhole.Type = FormatType.Boolean;
        keyhole.Format = null;

        return base.WriteMutableValue(ref html, value);
    }

    public override bool WriteMutableValue<T>(ref Html html, T value, string? format = default)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
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

        return base.WriteMutableValue(ref html, value, format);
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null)
    {
        // end = h.end;

        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, expression);
    }

    public override bool WriteMutableAttribute<T>(ref Html html, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null)
        // where T : struct, IUtf8SpanFormattable // (from base)
    {
        // end = h.end;

        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, format, expression);
    }

    public override bool WriteMutableAttribute(ref Html html, ReadOnlySpan<char> attrName, Func<string, Html> attrValue, string? expression = null)
    {
        // end = h.end;

        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = ...TBD.. // actually I DO know it since this is executing AFTER the HTML instantiates
        // keyhole.Integer = h.start;
        keyhole.Type = FormatType.Attribute;
        keyhole.String = expression;

        // ref var start = ref keyholes[h.start];
        // start.Integer = Cursor;

        return base.WriteMutableAttribute(ref html, attrName, attrValue, expression);
    }

    public override bool WriteEventHandler(ref Html html, Action eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Action eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Action<Event> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Func<Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Func<Event, Task> eventHandler, string? expression = null) => WriteEventHandler(ref html, expression);
    private bool WriteEventHandler(ref Html html, string? expression = null)
    {
        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        keyhole.Type = FormatType.EventHandler;
        keyhole.String = expression;

        return CompleteFormattedValue();
    }

    public override bool WriteMutableElement<TView>(ref Html html, TView view) => WriteMutableElement(ref html, view.Render());
    public override bool WriteMutableElement(ref Html html, Slot slot) => WriteMutableElement(ref html, slot());
    public override bool WriteMutableElement(ref Html html, Html partial, string? expression = null)
    {
        Keymaker.MoveUp(1);

        ref var keyhole = ref keyholes[html.Index + html.Cursor];
        keyhole.Key = Keymaker.GetNext();
        keyhole.OldId = Cursor;
        // keyhole.RefId = parentStartIndex;
        keyhole.Type = FormatType.Html;
        keyhole.String = expression;
        keyhole.Integer = partial.Index;
        keyhole.Long = partial.Cursor;

        // // Update the "starting end cap" to point its end.
        // ref var start = ref keyholes[parentStartIndex];
        // start.Integer = Cursor;

        return base.WriteMutableElement(ref html, partial);
    }

    public IEnumerable<Keyhole> EnumerateDepthFirst(Keyhole html)
    {
        var start = html.Integer!.Value;
        var end = start + html.Long!.Value - 1;
        for (int i = start; i <= end; i++)
        {
            var keyhole = keyholes[i];
            yield return keyhole;
            
            if (keyhole.Type == FormatType.Html)
            {
                foreach (var k in EnumerateDepthFirst(keyhole))
                {
                    yield return k;
                }
            }
        }
    }
}