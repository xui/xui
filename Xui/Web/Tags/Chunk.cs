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
    public double? Double;
    public bool? Boolean;
    public DateTime? DateTime;
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
            case FormatType.Double:
                return c1.Double == c2.Double && c1.Format == c2.Format;
            case FormatType.DateTime:
                return c1.DateTime == c2.DateTime && c1.Format == c2.Format;
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