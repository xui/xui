using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Web4.Composers;

namespace Web4;

public static class Debug
{
    const string cssDefault = "font-weight:normal;font-family:monospace,monospace;";
    const string cssVariable = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
    const string cssFunction = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
    const string cssHtml = "font-weight:normal;font-family:monospace,monospace;";
    const string cssNumber = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
    const string cssString = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
    const string cssType = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
    const string cssOperator = "font-weight:normal;font-family:monospace,monospace;";
    const string cssLiteral = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
    const string cssNotes = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
    const string cssLink = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
    const string cssBrace = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";

    internal static async ValueTask Write(WebSocketWriter writer, Snapshot before, Snapshot after, CancellationToken cancellationToken)
    {
        await writer.Write('[');

        await writer.WriteRpc("console.groupCollapsed", "Server Diff (2)");
        await writer.Write(',');
        await writer.WriteRpc("console.log", "%cDEBUG output is default-enabled for localhost\\nManually configure using server.debug = [true | false]", cssNotes);
        await writer.Write(',');

        // for (int i = 0; i < after.Root.Length; i++)
        // {
        //     ref Keyhole keyhole = ref after.Buffer[i];
        //     Append(output, i, ref keyhole, after.Buffer);
        // }

        await writer.WriteRpc("console.log", "\\n%cBenchmark this shell:\\n%c› %cserver.%cbenchmark%c();", cssType, cssVariable, cssDefault, cssFunction, cssDefault);
        await writer.Write(',');
        await writer.WriteRpc("console.groupEnd");

        await writer.Write(']');
        await writer.Flush(cancellationToken);
    }

    private static void Append(StringBuilder output, int index, ref Keyhole keyhole, Span<Keyhole> keyholes)
    {
        switch (keyhole.Type)
        {
            case FormatType.StringLiteral:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%c ",-24} 🟢 %c\`{InlineString(keyhole.String)}\``, cssVariable, cssLiteral);
                        console.log(`\n{keyhole.String}\n\n`);
                    console.groupEnd();
                    """);
                break;
            case FormatType.String:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{keyhole.String}'`, cssVariable, cssOperator, cssType, cssString);
                    console.groupEnd();
                    """);
                break;
            case FormatType.Integer:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
                    console.groupEnd();
                    """);
                break;
            case FormatType.EventListener:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace);
                    console.groupEnd();
                    """);
                break;
            case FormatType.Attribute:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace);
                    console.groupEnd();
                    """);
                break;
            case FormatType.Html:
                int start = keyhole.Integer;
                int length = (int)keyhole.Long;
                if (keyhole.Key != string.Empty)
                {
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" } %cbuffer[{start}..{start + length - 1}]`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace, cssLink);
                        """);
                }
                else
                {
                    output.AppendLine($"""
                        console.groupCollapsed(`{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" } %cbuffer[{start}..{start + length - 1}]`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace, cssLink);
                        """);
                }

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref keyholes[i];
                    Append(output, i, ref k, keyholes);
                }
                output.AppendLine($"""
                    console.groupEnd();
                    """);
                break;
            default:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
                    console.log(`
                    SET /app XTTP/0.1
                    Host: myapp.ui.cloud
                    User-Agent: neato
                    Content-Type: application/x-www-form-urlencoded
                    Content-Length: 50

                    keyAFBC={keyhole.DateTime}&keyAbBC={keyhole.DateTime}

                    `);
                    console.groupEnd();
                    """);
                break;
        }
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