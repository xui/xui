using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Routing.Tree;

namespace Xui.Web.Html;

internal struct Chunk
{
    public int Id;
    public FormatType Type;
    public string? String;
    public int? Integer;
    public long? Long;
    public float? Float;
    public double? Double;
    public decimal? Decimal;
    public bool? Boolean;
    public DateTime? DateTime;
    public TimeSpan? TimeSpan;
    public IView? View;
    public string? Format;
    public Action Action;
    public Action<Event> ActionEvent;
    public Func<Task> ActionAsync;
    public Func<Event, Task> ActionEventAsync;

    public static bool operator ==(Chunk c1, Chunk c2)
    {
        if (c1.Type != c2.Type)
            return false;

        switch (c1.Type)
        {
            case FormatType.StringLiteral:
                return c1.String == c2.String;
            case FormatType.String:
                return c1.String == c2.String && c1.Format == c2.Format;
            case FormatType.Integer:
                return c1.Integer == c2.Integer && c1.Format == c2.Format;
            case FormatType.Long:
                return c1.Long == c2.Long && c1.Format == c2.Format;
            case FormatType.Float:
                return c1.Float == c2.Float && c1.Format == c2.Format;
            case FormatType.Double:
                return c1.Double == c2.Double && c1.Format == c2.Format;
            case FormatType.Decimal:
                return c1.Decimal == c2.Decimal && c1.Format == c2.Format;
            case FormatType.DateTime:
                return c1.DateTime == c2.DateTime && c1.Format == c2.Format;
            case FormatType.TimeSpan:
                return c1.TimeSpan == c2.TimeSpan && c1.Format == c2.Format;
            case FormatType.Boolean:
                return c1.Boolean == c2.Boolean && c1.Format == c2.Format;
            case FormatType.View:
                return c1.View == c2.View;
            case FormatType.HtmlString:
                // no-op
                return true;
        }

        return true;
    }

    public static bool operator !=(Chunk c1, Chunk c2)
    {
        return !(c1 == c2);
    }

    public override readonly bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}