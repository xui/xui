using System.Text;

namespace Xui.Web.Html;

public record struct Delta(
    int Id,
    DeltaType Type,
    StringBuilder Output
);

public enum DeltaType
{
    NodeValue,
    NodeAttribute,
    HtmlPartial,
}