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

public static class MapWeb4RouteExtensions
{
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
            var composer = XtmlComposer.Shared(pipeWriter, window);
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