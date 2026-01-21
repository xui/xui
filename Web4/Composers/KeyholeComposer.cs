using System.Drawing;

namespace Web4.Composers;

public abstract class KeyholeComposer : BaseComposer
{
    protected readonly KeyCursor keyCursor = new();
    public byte[] Key { get; private set; } = [];

    public virtual bool OnTemplateBegin(ref Html html, ref string literal) => true;
    public virtual bool OnTemplateEnd(ref Html html) => true;

    public override bool OnMarkup(ref Html parent, string literal)
    {
        _ = (parent.Cursor, parent.Type) switch {
            (0, HtmlType.Template) => OnTemplateBegin(ref parent, ref literal),
            (0, HtmlType.Element) => OnHtmlBegin(ref parent),
            _ => true,
        };

        return true;
    }

    public override bool OnStringKeyhole(ref Html parent, string value) => OnKeyhole();
    public override bool OnBoolKeyhole(ref Html parent, bool value) => OnKeyhole();
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnKeyhole();
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnKeyhole();
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnKeyhole();
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => OnKeyhole();
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnKeyhole();
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnKeyhole();
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnKeyhole();
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnKeyhole();
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnKeyhole();
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => OnKeyhole();
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => OnKeyhole();

    public virtual bool OnHtmlBegin(ref Html html)
    {
        Key = keyCursor.MoveNext();
        keyCursor.MoveDown();
        return true;
    }

    public override bool OnHtmlKeyhole(ref Html parent, scoped Html html, string? format = null, string? expression = null)
        => html.Type == HtmlType.Template
            ? OnTemplateEnd(ref html)
            : OnHtmlEnd(ref parent, html, format, expression);

    public virtual bool OnHtmlEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        Key = keyCursor.MoveUp();
        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        Key = keyCursor.MoveNext();
        keyCursor.MoveDown();
        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        Key = keyCursor.MoveUp();
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => OnKeyhole();
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => OnKeyhole();
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => OnKeyhole();
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => OnKeyhole();

    public override void Reset()
    {
        keyCursor.Reset();
        base.Reset();
    }

    protected virtual bool OnKeyhole()
    {
        Key = keyCursor.MoveNext();
        return true;
    }
}