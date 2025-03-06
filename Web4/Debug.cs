using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Web4.Composers;

namespace Web4;

public static class Debug
{
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
    private static readonly string DEBOUNCE_MESSAGE = $"Server diff output limited to 1 per {DEBOUNCE_SECONDS} second(s)...";
    private static DateTime debounceUntil = DateTime.Now;

    internal static ValueTask Write(WebSocket webSocket, Snapshot before, Snapshot after, CancellationToken cancellationToken)
    {
        using var writer = new JsonRpcWriter(bufferSize: 2^12);

        if (debounceUntil > DateTime.Now)
        {
            writer.WriteRpc("console.log", DEBOUNCE_MESSAGE);
            return webSocket.SendAsync(writer.Memory, WebSocketMessageType.Text, true, cancellationToken);
        }
        debounceUntil = DateTime.Now.AddSeconds(DEBOUNCE_SECONDS);

        writer.BeginBatch();
        writer.WriteRpc("console.groupCollapsed", "Server diff (2)");
        writer.WriteRpc("console.log", "%cDEBUG output is default-enabled for localhost\\nManually configure using server.debug = [true | false]", CSS_NOTES);
        for (int index = 0; index < after.Root.Length; index++)
        {
            ref Keyhole keyhole = ref after.Buffer[index];
            switch (keyhole.Type)
            {
                case FormatType.StringLiteral:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c ",-24} 🟢 %c`{InlineString(keyhole.String) ?? ""}`", CSS_VARIABLE, CSS_LITERAL);
                    writer.WriteRpc("console.log", $"\\n{EscapeString(keyhole.String)}\\n\\n");
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.String:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{keyhole.String}'", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_STRING);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.Integer:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.EventListener:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.Attribute:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.Html:
                    int start = keyhole.Integer;
                    int length = (int)keyhole.Long;
                    if (keyhole.Key != string.Empty)
                    {
                        writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" } %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);
                    }
                    else
                    {
                        writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" } %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);
                    }

                    for (int i = start; i < start + length; i++)
                    {
                        var keyholes = after.Buffer;
                        ref var k = ref keyholes[i];
                        // Append(output, i, ref k, keyholes);
                    }
                    writer.WriteRpc("console.groupEnd");
                    break;
                default:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Double}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                    writer.WriteRpc("console.groupEnd");
                    break;
            }
        }
        writer.WriteRpc("console.log", "\\n%cBenchmark this shell:\\n%c› %cserver.%cbenchmark%c();", CSS_TYPE, CSS_VARIABLE, CSS_DEFAULT, CSS_FUNCTION, CSS_DEFAULT);
        writer.WriteRpc("console.groupEnd");
        writer.EndBatch();
        return webSocket.SendAsync(writer.Memory, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static string? InlineString(string? value)
    {
        int maxLength = 100;
        var inlined = value
            ?.Replace("\n", "")
            ?.Replace("\r", "") 
            ?.Replace("  ", "")
            ?? "";
        return (inlined.Length > maxLength)
            ? inlined[..(maxLength-3)] + "..."
            : inlined;
    }

    private static string? EscapeString(string? value)
    {
        return value
            ?.Replace("\"", "\\\"")
            ?.Replace("\n", "\\n")
            ?.Replace("\r", "")
            ?? "";
    }

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