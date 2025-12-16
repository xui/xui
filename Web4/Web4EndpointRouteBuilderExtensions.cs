#pragma warning disable IDE0130 // Namespace does not match folder structure

using Web4.Composers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Web4;
using Microsoft.AspNetCore.WebSockets;
using Html = Web4.Html;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Web4.WebSockets;

// TODO: "Don't place extension methods in the Microsoft.Extensions.DependencyInjection namespace unless you're authoring an official Microsoft package": https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage#register-services-for-di
// TODO: Html namespace collision problem?
//namespace Microsoft.AspNetCore.Builder;
namespace Microsoft.Extensions.DependencyInjection;

public static class Web4EndpointRouteBuilderExtensions
{
    public static void UseXtmlFiles(this IEndpointRouteBuilder endpoints)
    {
        // TODO: Implement.
    }

    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    public static IEndpointConventionBuilder MapGet(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        Func<Html> template)
    {
        return MapGet(endpoints, pattern, httpContext => template());
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
            var composer = new HtmlComposer(pipeWriter); // TODO: Memory allocation
            await pipeWriter.WriteAsync(composer, $"{template(httpContext)}");

        });
    }

    public static WindowBuilder Map(
        this WebApplication app,
        [StringSyntax("Route")] string pattern,
        Func<Html> html)
        => MapWeb4(app, pattern, html);

    public static WindowBuilder MapWeb4(
        this WebApplication app,
        [StringSyntax("Route")] string pattern,
        Func<Html> template)
    {
        app.UseWebSockets();
        var group = app.MapGroup(pattern);
        var window = new WindowBuilder(group, template);

        group.Map("/", async httpContext =>
        {
            var pipeWriter = httpContext.Response.BodyWriter;
            var composer = new XtmlComposer(pipeWriter, window); // TODO: Memory allocation
            await pipeWriter.WriteAsync(composer, window.Template, httpContext);
        });

        group.Map("/web4", async httpContext =>
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var logger = app.Services.GetRequiredService<ILogger<WebSocketTransport>>();
                await WebSocketTransport.Bind(
                    httpContext,
                    window,
                    logger,
                    app.Lifetime.ApplicationStopping
                );
            }
        });

        group.Map("/web4/alive", async httpContext =>
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
                await httpContext.WebSockets.AcceptWebSocketAsync();
        });


        return window;
    }
}