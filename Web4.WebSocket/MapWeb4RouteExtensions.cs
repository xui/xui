#pragma warning disable IDE0130 // Namespace does not match folder structure

using Web4.Keyholes.Composers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Web4.Keyholes;
using Html = MicroHtml.Html;
using System.Diagnostics;
using Web4.WebSocket;
using System.IO.Pipelines;
using MicroHtml;
using MicroHtml.Composers;

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
            var composer = HtmlKeyComposer.Reuse(pipeWriter, window);
            await httpContext.WriteAsync(composer, window.Template);
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

    private static ValueTask<FlushResult> WriteAsync<T>(
        this HttpContext httpContext,
        T composer,
        Func<Html> template,
        bool includeServerTiming = false) // TODO: Move `includeServerTiming` to Config
            where T : BaseComposer, IStreamingComposer
    {
        var pipeWriter = httpContext.Response.BodyWriter;
        if (!includeServerTiming)
        {
            pipeWriter.Write(composer, $"{template()}");
            return pipeWriter.FlushAsync(httpContext.RequestAborted);
        }
        else
        {
            long gc1 = GC.GetAllocatedBytesForCurrentThread();
            long stopwatch = Stopwatch.GetTimestamp();

            pipeWriter.Write(composer, $"{template()}");

            var elapsed = Stopwatch.GetElapsedTime(stopwatch);
            long gc2 = GC.GetAllocatedBytesForCurrentThread();

            // This allocates.  Boo!  But it occurs after measurement.
            httpContext.Response.Headers["Server-Timing"] = $"""
                allocations;desc="Allocations: {gc2 - gc1}b", render;desc="Web4.Render";dur={elapsed.TotalNanoseconds / 1_000_000d}
                """;

            return pipeWriter.FlushAsync(httpContext.RequestAborted);
        }
    }
}