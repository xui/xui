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
using Web4.Transports;

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
        Func<Html> requestDelegate)
    {
        return MapGet(endpoints, pattern, context => requestDelegate());
    }
    
    public static IEndpointConventionBuilder MapGet(
        this IEndpointRouteBuilder endpoints, 
        [StringSyntax("Route")] string pattern, 
        Func<HttpContext, Html> requestDelegate)
    {
        return endpoints.Map(pattern, async http =>
        {
            var response = http.Response;
            if (!response.HasStarted)
            {
                response.ContentLength = HotSpot.GetContentLengthIfConst(pattern);
                var startAsyncTask = response.StartAsync();
                if (!startAsyncTask.IsCompletedSuccessfully)
                {
                    await startAsyncTask;
                }
            }

            var pipeWriter = response.BodyWriter;
            var composer = new HtmlComposer(pipeWriter);
            await pipeWriter.WriteAsync(composer, $"{requestDelegate(http)}");

            HotSpot.Track(pattern, composer);
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
        Func<Html> html)
    {
// TODO: Move to config, then simplify your pipeWriter.WriteAsync(...).
#if DEBUG
        bool includeServerTiming = true;
#else
        bool includeServerTiming = false;
#endif

        app.UseWebSockets();
        var group = app.MapGroup(pattern);
        var windowBuilder = new WindowBuilder(group, html);

        group.MapDebugOutput();

        group.Map("/", async http =>
        {
            if (http.WebSockets.IsWebSocketRequest)
            {
                var transport = await WebSocketTransport.Create(http);
                var window = transport.GetOrCreateWindow(windowBuilder);
                await transport.ListenForRpcMessages(window);
            }
            else
            {
                var pipeWriter = http.Response.BodyWriter;
                var composer = new XtmlComposer(pipeWriter, windowBuilder);
                await pipeWriter.WriteAsync(composer, windowBuilder.Html, http, includeServerTiming, http.RequestAborted);
            }
        });

        return windowBuilder;
    }
}