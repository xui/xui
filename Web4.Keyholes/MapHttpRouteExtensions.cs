using Web4.Composers;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using MicroHtml;
using Html = MicroHtml.Html;

// TODO: "Don't place extension methods in the Microsoft.Extensions.DependencyInjection namespace unless you're authoring an official Microsoft package": https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage#register-services-for-di
// TODO: Html namespace collision problem?
//namespace Microsoft.AspNetCore.Builder;
namespace Microsoft.Extensions.DependencyInjection;

public static class MapHttpRouteExtensions
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