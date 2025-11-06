using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
partial class WebSocketTransport : IKeyholes
{
    public void SetTextNode(string key, ref Keyhole keyhole)
    {
        Output.WriteNotification(
            method: ("keyholes.", key, ".setTextNode"),
            param1: ref keyhole
        );
    }

    public void SetAttribute(string key, ref Keyhole keyhole)
    {
        Output.WriteNotification(
            method: ("keyholes.", key, ".setAttribute"),
            param1: ref keyhole
        );
    }

    public void SetAttribute(string key, Span<Keyhole> keyholes)
    {
        Output.WriteNotification(
            method: ("keyholes.", key, ".setAttribute"),
            param1: keyholes
        );
    }

    public void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes)
    {
        Output.WriteNotification(buffer,
            method: ("keyholes.", key, ".setElement"),
            param1: keyholes
        );
    }

    public void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, bool reverseTransition)
    {
        Output.WriteNotification(buffer,
            method: ("keyholes.", key, ".setElement"),
            param1: keyholes,
            param2: reverseTransition ? ("web4-rev-", key) : ("web4-fwd-", key)
        );
    }

    public void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, object oldTag, object newTag)
    {
        Output.WriteNotification(buffer,
            method: ("keyholes.", key, ".setElement"),
            param1: keyholes,
            param2: ("web4-move-", oldTag.GetHashCode()),
            param3: ("web4-move-", newTag.GetHashCode())
          );
    }

    public void AddElement(Keyhole[] buffer, string priorKey, string key, Span<Keyhole> keyholes)
    {
        Output.WriteNotification(buffer,
            method: ("keyholes.", priorKey, ".addElement"),
            param1: key,
            param2: keyholes
        );
    }

    public void AddElement(Keyhole[] buffer, string priorKey, string key, Span<Keyhole> keyholes, ValueTuple<string, string> viewTransitionName)
    {
        Output.WriteNotification(buffer,
            method: ("keyholes.", priorKey, ".addElement"),
            param1: key,
            param2: keyholes,
            param3: viewTransitionName
        );
    }

    public void RemoveElement(string key)
    {
        Output.WriteNotification(
            method: ("keyholes.", key, ".removeElement")
        );
    }

    public void RemoveElement(string key, ValueTuple<string, string> viewTransitionName)
    {
        Output.WriteNotification(
            method: ("keyholes.", key, ".removeElement"),
            param1: viewTransitionName
        );
    }

    public void DispatchEvent(Action listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            listener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public void DispatchEvent<T>(Action<Event> listener, T @event)
        where T : struct, Event
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            listener(@event);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public async Task DispatchEvent(Func<Task> listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            await listener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public async Task DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct, Event
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            await listener(@event);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public void Ping()
    {
        var stopwatch = LogMessageStarted("ping");
        LogMessageEnded("ping", stopwatch);
    }

    public void Dump()
    {
        var stopwatch = LogMessageStarted("dump");

        var buffer = CaptureSnapshot();
        new KeyholeDumper(Console, buffer).Dump();

        LogMessageEnded("dump", stopwatch);
    }

    public void Benchmark(int? threads)
    {
        var stopwatch = LogMessageStarted("benchmark");

        // TODO: Implement

        LogMessageEnded("benchmark", stopwatch);
    }

    private Stopwatch? LogMessageStarted(string method)
    {
        // TODO: Temporary until a proper IHttpApplication is ready to be built and include proper diagnostics.

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Request starting {Protocol} {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} - {ContentType} {ContentLength}",
                "JSON-RPC/2.0",
                method,
                httpContext.Request.Scheme == "https" ? "wss" : "ws",
                httpContext.Request.Host,
                httpContext.Request.PathBase,
                httpContext.Request.Path,
                httpContext.Request.QueryString,
                "application/json",
                ""
            );
            return Stopwatch.StartNew();
        }
        return null;
    }

    private void LogMessageEnded(string method, Stopwatch? stopwatch)
    {
        // TODO: Temporary until a proper IHttpApplication is ready to be built and include proper diagnostics.

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Request finished {Protocol} {Method} {Scheme}://{Host}{PathBase}{Path}{QueryString} - {StatusCode} {ContentLength} {ContentType} {ElapsedMilliseconds}ms",
                "JSON-RPC/2.0",
                method,
                httpContext.Request.Scheme == "https" ? "wss" : "ws",
                httpContext.Request.Host,
                httpContext.Request.PathBase,
                httpContext.Request.Path,
                httpContext.Request.QueryString,
                200,
                "",
                "application/json",
                stopwatch?.Elapsed.TotalMilliseconds
            );
        }
    }
}
