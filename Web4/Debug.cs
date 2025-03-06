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
    private const string cssDefault = "font-weight:normal;font-family:monospace,monospace;";
    private const string cssVariable = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
    private const string cssFunction = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
    private const string cssHtml = "font-weight:normal;font-family:monospace,monospace;";
    private const string cssNumber = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
    private const string cssString = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
    private const string cssType = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
    private const string cssOperator = "font-weight:normal;font-family:monospace,monospace;";
    private const string cssLiteral = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
    private const string cssNotes = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
    private const string cssLink = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
    private const string cssBrace = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";

    private const int DEBOUNCE_SECONDS = 5;
    private static DateTime debounceUntil = DateTime.Now;

    internal static ValueTask Write(WebSocket webSocket, Snapshot before, Snapshot after, CancellationToken cancellationToken)
    {
        using var writer = new JsonRpcWriter(bufferSize: 2^12);

        if (debounceUntil > DateTime.Now)
        {
            writer.WriteRpc("console.log", $"Server diff output limited to 1 per {DEBOUNCE_SECONDS} second(s)...");
            return webSocket.SendAsync(writer.Memory, WebSocketMessageType.Text, true, cancellationToken);
        }
        debounceUntil = DateTime.Now.AddSeconds(DEBOUNCE_SECONDS);

        writer.BeginBatch();
        writer.WriteRpc("console.groupCollapsed", "Server diff (2)");
        writer.WriteRpc("console.log", "%cDEBUG output is default-enabled for localhost\\nManually configure using server.debug = [true | false]", cssNotes);
        for (int index = 0; index < after.Root.Length; index++)
        {
            ref Keyhole keyhole = ref after.Buffer[index];
            switch (keyhole.Type)
            {
                case FormatType.StringLiteral:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c ",-24} 🟢 %c`{InlineString(keyhole.String) ?? ""}`", cssVariable, cssLiteral);
                    writer.WriteRpc("console.log", $"\\n{EscapeString(keyhole.String)}\\n\\n");
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.String:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{keyhole.String}'", cssVariable, cssOperator, cssType, cssString);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.Integer:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}", cssVariable, cssOperator, cssType, cssNumber);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.EventListener:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }", cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.Attribute:
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }", cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace);
                    writer.WriteRpc("console.groupEnd");
                    break;
                case FormatType.Html:
                    int start = keyhole.Integer;
                    int length = (int)keyhole.Long;
                    if (keyhole.Key != string.Empty)
                    {
                        writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" } %cbuffer[{start}..{start + length - 1}]", cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace, cssLink);
                    }
                    else
                    {
                        writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 { $"%c{{ %c{EscapeString(keyhole.String)} %c}}" } %cbuffer[{start}..{start + length - 1}]", cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace, cssLink);
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
                    writer.WriteRpc("console.groupCollapsed", $"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Double}", cssVariable, cssOperator, cssType, cssNumber);
                    writer.WriteRpc("console.groupEnd");
                    break;
            }
        }
        writer.WriteRpc("console.log", "\\n%cBenchmark this shell:\\n%c› %cserver.%cbenchmark%c();", cssType, cssVariable, cssDefault, cssFunction, cssDefault);
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