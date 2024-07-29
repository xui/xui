using System.IO.Pipelines;

namespace Xui.Web.Html;

internal static class HtmlStringExtensions
{
    public static int GetContentLength(this HtmlString htmlString)
    {
        // TODO: Optimize
        var body = htmlString.ToStringWithExtras();
        return body.Length;
    }

    public static ValueTask<FlushResult> WriteAsync(this PipeWriter writer, ref HtmlString htmlString, CancellationToken cancellationToken = default)
    {
        // TODO: Optimize
        var body = htmlString.ToStringWithExtras();
        ReadOnlyMemory<byte> memory = new(System.Text.Encoding.UTF8.GetBytes(body));
        
        return writer.WriteAsync(memory, cancellationToken);
    }
}