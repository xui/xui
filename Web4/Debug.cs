using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web4.Composers;

namespace Web4;

public static class Debug
{
    #if !DEBUG

    public const string JS = "";
    internal static void MapOutput(RouteGroupBuilder group) { }
    internal static async ValueTask Log(Keyhole[] before, Keyhole[] after) { }
    // TODO: ^ Empty body still has perf cost.  Make this a zero-cost abstraction.

    #else

    // TODO: This does not consider subroutes that might exist
    public const string JS = """

        let path = window.location.pathname;
        if (path.endsWith('/')) path = path.substring(0, path.length - 1);
        const eventSource = new EventSource(path + "/__debug");
        eventSource.onmessage = (e) => clientRpc(e.data);
        eventSource.onerror = () => eventSource.close();
        """;

    private const string CSS_DEFAULT = "font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_VARIABLE = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_FUNCTION = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_HTML = "font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_NUMBER = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_STRING = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_TYPE = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_OPERATOR = "font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_LITERAL = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_NOTES = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_LINK = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
    private const string CSS_BRACE = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";

    private const int DEBOUNCE_SECONDS = 1;
    private static DateTime debounceUntil = DateTime.Now;
    private static HttpContext http;

    private record JsonRpc(string method, string[] @params, string jsonrpc = "2.0");

    internal static void MapDebugOutput(this RouteGroupBuilder group)
    {
        group.MapGet("__debug", async http =>
        {
            Debug.http = http;
            http.Response.Headers.ContentType = "text/event-stream";
            
            await Log(new JsonRpc("console.log", ["Server diff output established"]));

            await Task.Delay(Timeout.Infinite, http.RequestAborted);
        });
    }

    private static async Task Log(JsonRpc message)
    {
        await http.Response.WriteAsync("data: ");
        await http.Response.WriteAsync(JsonSerializer.Serialize(message));
        await http.Response.WriteAsync("\n\n");
        await http.Response.Body.FlushAsync(http.RequestAborted);
    }

    private static async Task Log(params IEnumerable<JsonRpc> messages)
    {
        await http.Response.WriteAsync("data: ");
        await http.Response.WriteAsync(JsonSerializer.Serialize(messages));
        await http.Response.WriteAsync("\n\n");
        await http.Response.Body.FlushAsync(http.RequestAborted);
    }

    internal static async ValueTask Log(Keyhole[] before, Keyhole[] after)
    {
        var cancel = http.RequestAborted;

        if (debounceUntil > DateTime.Now)
        {
            await Log(new JsonRpc("console.log", [$"Server diff output is debounced for {DEBOUNCE_SECONDS} second(s)..."]));
            return;
        }

        debounceUntil = DateTime.Now.AddSeconds(DEBOUNCE_SECONDS);

        var messages = new List<JsonRpc>
        {
            new("console.groupCollapsed", ["Server diff (2)"]),
            new("console.log", ["%cDEBUG output is default-enabled for localhost\nManually configure using server.debug = [true | false]", CSS_NOTES])
        };

        var rootLength = after[0].Length;
        for (int index = 0; index < rootLength; index++)
        {
            ref Keyhole keyhole = ref after[index];
            messages.AddRange(Write(index, keyhole, before, after));
        }

        messages.Add(new("console.log", ["\n%cBenchmark this shell:\n%c› %cserver.%cbenchmark%c();", CSS_TYPE, CSS_VARIABLE, CSS_DEFAULT, CSS_FUNCTION, CSS_DEFAULT]));
        messages.Add(new("console.groupEnd", []));

        await Log(messages);
    }
    
    private static IEnumerable<JsonRpc> Write(int index, Keyhole keyhole, Keyhole[] before, Keyhole[] after)
    {
        switch (keyhole.Type)
        {
            case FormatType.StringLiteral:
                yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c ",-24} 🟢 %c`{InlineString(keyhole.String)}`", CSS_VARIABLE, CSS_LITERAL]);
                yield return new("console.log", [$"\n{keyhole.String}\n\n"]);
                yield return new("console.groupEnd", []);
                break;
            case FormatType.String:
                yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{keyhole.String}'", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_STRING]);
                yield return new("console.groupEnd", []);
                break;
            case FormatType.Integer:
                yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER]);
                yield return new("console.groupEnd", []);
                break;
            // TODO: Support the other FormatTypes too
            case FormatType.EventListener:
                yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE]);
                yield return new("console.groupEnd", []);
                break;
            case FormatType.Attribute:
                yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE]);
                yield return new("console.groupEnd", []);
                break;
            case FormatType.Html:
                int start = keyhole.Integer;
                int length = (int)keyhole.Length;
                if (keyhole.Key != string.Empty)
                {
                    yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" } %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK]);
                }
                else
                {
                    yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" } %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK]);
                }

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref after[i];
                    foreach (var m in Write(i, k, before, after))
                        yield return m;
                }
                yield return new("console.groupEnd", []);
                break;
            default:
                yield return new("console.groupCollapsed", [$"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Double}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER]);
                yield return new("console.groupEnd", []);
                break;
        }
    }

    private static string InlineString(string? value)
    {
        if (value is null)
            return "";

        int maxLength = 100;
        var inlined = new StringBuilder(value)
            .Replace("\n", "")
            .Replace("\r", "") 
            .Replace("  ", "")
            .ToString();
        return (inlined.Length > maxLength)
            ? inlined[..(maxLength-3)] + "..."
            : inlined;
    }
    
    #endif

    public static IDisposable PerfCheck(string name = "unnamed") => new Perf(name);

    private class Perf(string name) : IDisposable
    {
        readonly long gc1 = GC.GetAllocatedBytesForCurrentThread();
        readonly long sw1 = Stopwatch.GetTimestamp();

        public void Dispose()
        {
            var elapsed = Stopwatch.GetElapsedTime(sw1);
            long gc2 = GC.GetAllocatedBytesForCurrentThread();
            Console.WriteLine($"{$"🚥 Perf({name}):",-35} elapsed:{$"{elapsed.TotalNanoseconds:n0} ns",-15} {$"allocations: {(gc2 - gc1):n0} bytes",-25}   thread:{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}