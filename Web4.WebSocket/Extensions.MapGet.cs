using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using MicroHtml;

namespace Web4.WebSocket;

public static partial class Extensions
{
    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    public static IEndpointConventionBuilder MapGet(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        Func<Html> template)
    {
        return endpoints.Map(pattern, async httpContext =>
        {
            // TODO: Optimization: set ContentLength if Html's formattedCount is zero.

            var pipeWriter = httpContext.Response.BodyWriter;
            pipeWriter.Write($"{template()}");
            await pipeWriter.FlushAsync(httpContext.RequestAborted);
        });
    }

    public static IEndpointConventionBuilder MapGet(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        Func<HttpContext, Html> template)
    {
        return endpoints.Map(pattern, async httpContext =>
        {
            // TODO: Optimization: set ContentLength if Html's formattedCount is zero.

            var pipeWriter = httpContext.Response.BodyWriter;
            pipeWriter.Write($"{template(httpContext)}");
            await pipeWriter.FlushAsync(httpContext.RequestAborted);
        });
    }
}