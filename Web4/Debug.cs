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
    internal static async ValueTask Log(Snapshot before, Snapshot after) { }
    // TODO: ^ Empty body still has perf cost.  Make this a zero-cost abstraction.

    #else

    // TODO: This does not consider subroutes that might exist
    public const string JS = """

        const eventSource = new EventSource(window.location.pathname + "/__debug");
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

    private const int DEBOUNCE_SECONDS = 5;
    private static DateTime debounceUntil = DateTime.Now;
    private static HttpContext http;

    internal static void MapOutput(RouteGroupBuilder group)
    {
        group.MapGet("__debug", async http =>
        {
            http.Response.Headers.ContentType = "text/event-stream";
            Debug.http = http;
            
            await http.Response.WriteAsync("data: ");
            await http.Response.WriteAsync("""
            {"jsonrpc":"2.0", "method":"console.log", "params":["Server diff output established"]}
            """);
            await http.Response.WriteAsync("\n\n");
            await http.Response.Body.FlushAsync(http.RequestAborted);

            await Task.Delay(Timeout.Infinite, http.RequestAborted);
        });
    }

    internal static async ValueTask Log(Snapshot before, Snapshot after)
    {
        var cancel = http.RequestAborted;

        if (debounceUntil > DateTime.Now)
        {
            await http.Response.WriteAsync("data: ");
            await http.Response.WriteAsync("""
                {"jsonrpc":"2.0","method":"console.log","params":["Server diff output is debounced for 5 second(s)..."]}
                """);
            await http.Response.WriteAsync("\n\n");
            await http.Response.Body.FlushAsync(cancel);
            return;
        }

        debounceUntil = DateTime.Now.AddSeconds(DEBOUNCE_SECONDS);

        await http.Response.WriteAsync("data: [");

        await http.Response.WriteAsync("""
            {"jsonrpc":"2.0","method":"console.groupCollapsed","params":["Server diff (2)"]}
            """);
        await WriteRpc("console.log", "%cDEBUG output is default-enabled for localhost\\nManually configure using server.debug = [true | false]", CSS_NOTES);

        for (int index = 0; index < after.Root.Length; index++)
        {
            ref Keyhole keyhole = ref after.Buffer[index];
            await Write(index, keyhole, before, after);
        }

        await WriteRpc("console.log", "\\n%cBenchmark this shell:\\n%c› %cserver.%cbenchmark%c();", CSS_TYPE, CSS_VARIABLE, CSS_DEFAULT, CSS_FUNCTION, CSS_DEFAULT);
        await WriteRpc("console.groupEnd");

        await http.Response.WriteAsync("]\n\n");
        await http.Response.Body.FlushAsync(cancel);
    }
    
    private static async Task Write(int index, Keyhole keyhole, Snapshot before, Snapshot after)
    {
        switch (keyhole.Type)
        {
            case FormatType.StringLiteral:
                await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c ",-24} 🟢 %c`{InlineString(keyhole.String)}`", CSS_VARIABLE, CSS_LITERAL);
                await WriteRpc("console.log", $"\\n{EscapeString(keyhole.String)}\\n\\n");
                await WriteRpc("console.groupEnd");
                break;
            case FormatType.String:
                await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{keyhole.String}'", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_STRING);
                await WriteRpc("console.groupEnd");
                break;
            case FormatType.Integer:
                await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                await WriteRpc("console.groupEnd");
                break;
            // TODO: Support the other FormatTypes too
            case FormatType.EventListener:
                await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" }", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                await WriteRpc("console.groupEnd");
                break;
            case FormatType.Attribute:
                await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" }", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                await WriteRpc("console.groupEnd");
                break;
            case FormatType.Html:
                int start = keyhole.Integer;
                int length = (int)keyhole.Length;
                if (keyhole.Key != string.Empty)
                {
                    await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" } %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);
                }
                else
                {
                    await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" } %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);
                }

                for (int i = start; i < start + length; i++)
                {
                    var keyholes = after.Buffer;
                    ref var k = ref keyholes[i];
                    await Write(i, k, before, after);
                }
                await WriteRpc("console.groupEnd");
                break;
            default:
                await WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Double}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                await WriteRpc("console.groupEnd");
                break;
        }
    }

    public static async Task WriteRpc(string method, params string[] args)
    {
        await http.Response.WriteAsync(",");

        await http.Response.WriteAsync("""
            {"jsonrpc":"2.0","method":"
            """);
        await http.Response.WriteAsync(method);
        await http.Response.WriteAsync("""
            ","params":[
            """);
        for (int i = 0; i < args.Length; i++)
        {
            await http.Response.WriteAsync(i == 0 ? "\"" : ",\"");
            await http.Response.WriteAsync(args[i]);
            await http.Response.WriteAsync("\"");
        }
        await http.Response.WriteAsync("]}");
    }

    private static string InlineString(string? value)
    {
        if (value is null)
            return "";

        int maxLength = 100;
        var inlined = new StringBuilder(value)
            .Replace("\"", "\\\"")
            .Replace("\n", "")
            .Replace("\r", "") 
            .Replace("  ", "")
            .ToString();
        return (inlined.Length > maxLength)
            ? inlined[..(maxLength-3)] + "..."
            : inlined;
    }

    private static string EscapeString(string? value)
    {
        if (value is null)
            return "";

        return new StringBuilder(value)
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "")
            .ToString();
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
            Console.WriteLine($"{$"🚥 Perf({name}):",-35} elapsed:{$"{elapsed.TotalNanoseconds:n0} ns",-15} allocations: {(gc2 - gc1):n0} bytes");
        }
    }
}