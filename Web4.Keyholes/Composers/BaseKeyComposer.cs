using System.Drawing;
using MicroHtml;
using MicroHtml.Composers;
using Web4.Dom;

namespace Web4.Keyholes.Composers;

public abstract class BaseKeyComposer : BaseComposer
{
    protected readonly KeyCursor keyCursor = new();
    public byte[] Key { get; private set; } = [];
    private bool isHtmlPrepared = true;

    public override void Grow(int literalLength, int keyholeCount)
    {
        isHtmlPrepared = false;
        base.Grow(literalLength, keyholeCount);
    }

    public virtual bool OnTemplateBegin(ref Html html, ref string markup) => true;
    public virtual bool OnTemplateEnd(ref Html html) => true;

    public override bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1) => (isHtmlPrepared, parent.Type) switch {
        (false, HtmlType.Template) => isHtmlPrepared = OnTemplateBegin(ref parent, ref literal),
        (false, HtmlType.Element) => isHtmlPrepared = OnHtmlBegin(ref parent, relativeOrder),
        _ => true,
    };

    public override bool OnStringKeyhole(ref Html parent, string value) => OnKeyhole(ref parent);
    public override bool OnBoolKeyhole(ref Html parent, bool value) => OnKeyhole(ref parent);
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => OnKeyhole(ref parent);

    public virtual bool OnHtmlBegin(ref Html html, int relativeOrder = -1)
    {
        Key = keyCursor.MoveNext();
        keyCursor.MoveDown();
        return true;
    }

    public override bool OnHtmlKeyhole(ref Html parent, scoped Html html, int relativeOrder = -1, string? format = null, string? expression = null)
        => html.Type switch {
            HtmlType.Wrapper => true, // ignore
            HtmlType.Template => OnTemplateEnd(ref html),
            HtmlType.Element or _ => OnHtmlEnd(ref parent, html, relativeOrder, format, expression),
        };

    public virtual bool OnHtmlEnd(ref Html parent, scoped Html html, int relativeOrder = -1, string? format = null, string? expression = null)
    {
        Key = keyCursor.MoveUp();
        return true;
    }

    public virtual bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        Key = keyCursor.MoveNext();
        keyCursor.MoveDown();
        return true;
    }

    public override bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
        => OnIteratorBegin(ref parent, ref htmls, format, expression)
        && base.OnIteratorKeyhole(ref parent, ref htmls, enumerable, format, expression)
        && OnIteratorEnd(ref parent, ref htmls, format, expression);

    public virtual bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        Key = keyCursor.MoveUp();
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnKeyhole(ref parent);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnKeyhole(ref parent);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnKeyhole(ref parent);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnKeyhole(ref parent);

    public override void Reset()
    {
        isHtmlPrepared = true;
        keyCursor.Reset();
        base.Reset();
    }

    protected virtual bool OnKeyhole(ref Html parent)
    {
        var discard = string.Empty;
        _ = (isHtmlPrepared, parent.Type) switch {
            (false, HtmlType.Template) => isHtmlPrepared = OnTemplateBegin(ref parent, ref discard),
            (false, HtmlType.Element) => isHtmlPrepared = OnHtmlBegin(ref parent),
            _ => true,
        };

        Key = keyCursor.MoveNext();
        return true;
    }
}