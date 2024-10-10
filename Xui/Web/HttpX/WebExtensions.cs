using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Xui.Web;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Components.Web;
using Xui.Web.HttpX.Composers;

namespace Xui.Web.HttpX;

public delegate Html HtmlDelegate();
public delegate Html HtmlHttpContextDelegate(HttpContext httpContext);

public static class WebExtensions
{
    public static void AddXui(this IServiceCollection services)
    {
        services.AddWebSockets(options =>
        {
        });
    }

    // TODO: Figure out later, the clearest way to configure all the things.
    // This will include things like static site generation too, I believe.
    public static void AddXuiTags(this IServiceCollection services) => services.AddXui();
    public static void AddZeroScript(this IServiceCollection services) => services.AddXui();

    public static WebApplication MapUI<T>(
        this WebApplication app,
        [StringSyntax("Route")] string pattern,
        UI<T> ui) where T : IViewModel
    {
        app.UseStaticFiles();
        app.UseWebSockets();

        var group = app.MapGroup(pattern);
        ui.MapPages(group);

        return app;
    }

    [RequiresDynamicCode("This API may perform reflection on the supplied delegate and its parameters. These types may require generated code and aren't compatible with native AOT applications.")]
    [RequiresUnreferencedCode("This API may perform reflection on the supplied delegate and its parameters. These types may be trimmed if not directly referenced.")]
    public static IEndpointConventionBuilder MapGet(
        this IEndpointRouteBuilder endpoints, 
        [StringSyntax("Route")] string pattern, 
        HtmlDelegate requestDelegate)
    {
        return endpoints.Map(
            pattern,
            async httpContext => {
                var pipeWriter = httpContext.Response.BodyWriter;
                var eventHandlers = pipeWriter.Write($"{requestDelegate()}");
                await pipeWriter.FlushAsync();
            }
        );
    }
    
    public static IEndpointConventionBuilder MapGet(
        this IEndpointRouteBuilder endpoints, 
        [StringSyntax("Route")] string pattern, 
        HtmlHttpContextDelegate requestDelegate)
    {
        return endpoints.Map(
            pattern,
            async httpContext => {
                var pipeWriter = httpContext.Response.BodyWriter;
                var eventHandlers = pipeWriter.Write($"{requestDelegate(httpContext)}");
                await pipeWriter.FlushAsync();
            }
        );
    }

    public static RouteGroupBuilder MapHttpX(
        this IEndpointRouteBuilder endpoints, 
        [StringSyntax("Route")] string pattern, 
        HtmlDelegate html)
    {
        var group = endpoints.MapGroup(pattern);
        group.Map(
            "/",
            async httpContext => {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    // --- ws:// ---
                    // Here is the request for upgrading to a websocket connection.  
                    // Switch protocols and await the pipe reader which receives
                    // DOM events that have been bubbled up beyond the browser.

                    using (var pipe = await WebSocketPipe.Upgrade(httpContext))
                    {
                        var httpxContext = new HttpXContext(pipe);
                        await httpxContext.AwaitEventListeners(html, httpContext.RequestAborted);
                    }

                    var logger = endpoints.ServiceProvider.GetService<ILogger>();
                    logger?.LogInformation("WebSocket has disconnected.");
                }
                else if (
                    HttpXContext.Get(httpContext) is var httpxContext && 
                    httpxContext.IsWebSocketOpen)
                {
                    // --- 204 ---
                    // Looks like the browser already has the page AND a websocket.
                    // Respond with a 204 - No Content which will not alter the page.
                    // Then update the browser's route using window.history.pushState. After that, 
                    // execute this route's code (which contains no output, only state changes)
                    // This may or may not trigger mutations to be pushed to the browser.

                    httpContext.Response.StatusCode = 204; // No Content
                    await httpContext.Response.CompleteAsync();

                    await httpxContext.UpdatePath(httpContext.Request.Path);
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

                    var pipeWriter = httpContext.Response.BodyWriter;
                    var composer = new HttpXComposer(pipeWriter);
                    var eventHandlers = pipeWriter.Write(composer, $"{html()}");
                    await pipeWriter.FlushAsync();
                }
            }
        );
        return group;
    }

    public static void MapPage<T>(
        this RouteGroupBuilder group,
        UI<T> ui,
        [StringSyntax("Route")] string pattern,
        Action<UI<T>.HttpXContext> mutateState) where T : IViewModel
    {
        group.MapGet(pattern, async httpContext =>
        {
            var context = UI<T>.HttpXContext.Get(httpContext, ui);

            // Here is the request for a websocket connection.  
            // Switch protocols and await the event loop inside which reads from the stream.
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                await context.AssignWebSocket(httpContext.WebSockets, httpContext);
            }

            // Here is a "normal" request.  There is no websocket yet so we cannot push mutations.
            // Just respond with an old fashioned 200 response.
            else if (!context.IsWebSocketOpen)
            {
                using (context.ViewModel.Batch())
                {
                    mutateState(context);
                }
                await context.WriteResponseAsync(httpContext);
            }

            // Looks like the browser already has the page AND a websocket.
            // Respond with a 204 - No Content which will not alter the page.
            // Then push down the new route requested.  After that run the lambda which
            // may or may not cause trigger mutations to be pushed to the browser.
            else
            {
                httpContext.Response.StatusCode = 204; // No Content
                await httpContext.Response.CompleteAsync();

                var httpXContext = new HttpXContext(context.Pipe);
                await httpXContext.UpdatePath(httpContext.Request.Path);

                using (context.ViewModel.Batch())
                {
                    mutateState(context);
                }
            }
        });
    }

    public static void MapPage<T>(
        this RouteGroupBuilder group,
        UI<T> ui,
        [StringSyntax("Route")] string pattern,
        Func<UI<T>.HttpXContext, Task> mutateStateAsync) where T : IViewModel
    {
        group.MapGet(pattern, async httpContext =>
        {
            var context = UI<T>.HttpXContext.Get(httpContext, ui);

            // Here is the request for a websocket connection.  
            // Switch protocols and await the event loop inside which reads from the stream.
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                await context.AssignWebSocket(httpContext.WebSockets, httpContext);
            }

            // Here is a "normal" request.  There is no websocket yet so we cannot push mutations.
            // Just respond with an old fashioned 200 response.
            else if (!context.IsWebSocketOpen)
            {
                // Page routes are a common place to fetch async data that this "page" might need.
                // FOR MACHINES: They'd prefer to wait until that async data is fully resolved 
                // so they can receive the final state as a single 200 GET response.
                // FOR HUMANS: The best UX is to start by immediately sending a 200 GET with 
                // zero blocking and then push DOM mutations caused by any subsequent state changes.
                if (httpContext.IsBot())
                    await mutateStateAsync(context);
                else
                    _ = mutateStateAsync(context);

                await context.WriteResponseAsync(httpContext);
            }

            // Looks like the browser already has the page AND a websocket.
            // Respond with a 204 - No Content which will not alter the page.
            // Then push down the new route requested.  After that run the lambda which
            // may or may not cause trigger mutations to be pushed to the browser.
            else
            {
                httpContext.Response.StatusCode = 204; // No Content
                await httpContext.Response.CompleteAsync();

                var httpXContext = new HttpXContext(context.Pipe);
                await httpXContext.UpdatePath(httpContext.Request.Path);

                _ = mutateStateAsync(context);
            }
        });
    }

    static bool IsBot(this HttpContext httpContext)
    {
        string? userAgent = httpContext.Request.Headers.UserAgent;

        if (userAgent is null || string.IsNullOrWhiteSpace(userAgent)) return true;
        if (userAgent.Contains("bot", StringComparison.CurrentCultureIgnoreCase)) return true;
        if (userAgent.Contains("crawl", StringComparison.CurrentCultureIgnoreCase)) return true;
        if (userAgent.Contains("spider", StringComparison.CurrentCultureIgnoreCase)) return true;
        if (userAgent.Contains("curl", StringComparison.CurrentCultureIgnoreCase)) return true;

        return false;
    }

    internal static string GetHttpXSessionId(this HttpContext httpContext)
    {
        const string SESSION_KEY = "httpx_session";

        var sessionId = httpContext.Request.Cookies[SESSION_KEY];
        if (sessionId == null)
        {
            sessionId = Guid.NewGuid().ToString();
            httpContext.Response.Cookies.Append(SESSION_KEY, sessionId);
            // TODO: Expire cookie?
        }
        return sessionId;
    }
}
