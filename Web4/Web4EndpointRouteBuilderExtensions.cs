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
        return endpoints.Map(
            pattern,
            async context =>
            {
                var response = context.Response;
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
                var composer = new DefaultComposer(pipeWriter);
                await pipeWriter.WriteAsync(composer, $"{requestDelegate(context)}");

                HotSpot.Track(pattern, composer);
            }
        );
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
        app.UseWebSockets();

        var group = app.MapGroup(pattern);
        var windowBuilder = new WindowBuilder(group, html);
        group.Map(
            "/",
            async http =>
            {
                var cancel = http.RequestAborted;
                if (http.WebSockets.IsWebSocketRequest)
                {
                    // --- ws:// ---
                    // Here is the request for upgrading to a websocket connection.  
                    // Switch protocols and await the pipe reader which receives
                    // DOM events that have been bubbled up beyond the browser.

                    using (var window = await Window.Upgrade(http))
                    {
                        await window.ListenForEvents(windowBuilder, cancel);
                    }
                }
                else
                {
                    // --- 200 ---
                    // If in here, this is a "normal" GET request.
                    // There is no websocket yet so we cannot push mutations.
                    // Respond with an old fashioned 200 response with HTML.
                    // Note! 200/GETs never await for external data (e.g. from a DB).
                    // Instead, they render immediately with whatever state they have in RAM.  
                    // Once the browser receives this HTML it will upgrade to 
                    // bidirectional communication via websocket.  Then it will be
                    // ready to fetch external data, mutate its UI state, and react to it
                    // by pushing DOM mutation instructions to the browser.

                    #if DEBUG
                    await DebugResponse(http, html, windowBuilder);
                    #else
                    var pipeWriter = http.Response.BodyWriter;
                    var composer = new HttpXComposer(pipeWriter, window);
                    await pipeWriter.WriteAsync(composer, $"{html()}", cancel);
                    #endif
                }
            }
        );

        Web4.Debug.MapOutput(group);

        return windowBuilder;
    }

    private static async Task DebugResponse(HttpContext http, Func<Html> html, WindowBuilder window)
    {
        var pipeWriter = http.Response.BodyWriter;

        var composer = new HttpXComposer(pipeWriter, window);
        long gc1 = GC.GetAllocatedBytesForCurrentThread();
        long stopwatch = Stopwatch.GetTimestamp();

        pipeWriter.Write(composer, $"{html()}");

        var elapsed = Stopwatch.GetElapsedTime(stopwatch);
        long gc2 = GC.GetAllocatedBytesForCurrentThread();

        http.Response.Headers["Server-Timing"] = $"""
            allocations;desc="Allocations: {gc2-gc1}b", render;desc="Web4.Render";dur={elapsed.TotalNanoseconds / 1_000_000d}
            """;                    

        await pipeWriter.FlushAsync(http.RequestAborted);
    }
}