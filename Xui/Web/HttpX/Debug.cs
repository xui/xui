using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;
using Xui.Web.HttpX.Composers;

namespace Xui.Web.HttpX;

public static class Debug
{
    public static StringBuilder GetOutput(DiffComposer composer)
    {
        var output = new StringBuilder();
        var keyholes = composer.Keyholes;

        output.Append($"""
            var cssDefault = "font-weight:normal;font-family:monospace,monospace;";
            var cssVariable = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
            var cssFunction = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
            var cssHtml = "font-weight:normal;font-family:monospace,monospace;";
            var cssNumber = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
            var cssString = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
            var cssType = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
            var cssOperator = "font-weight:normal;font-family:monospace,monospace;";
            var cssLiteral = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
            var cssNotes = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
            var cssLink = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
            var cssBrace = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";

            console.groupCollapsed("Server Diff\n%c(expand for details)", cssNotes);
            """);

        for (int i = 0; i < composer.RootLength; i++)
        {
            ref Keyhole keyhole = ref keyholes[i];
            Append(output, i, ref keyhole, keyholes);
        }

        output.Append($"""
            console.log("\n%cBenchmark this shell:\n%c› %cserver.%cbenchmark%c();", cssType, cssVariable, cssDefault, cssFunction, cssDefault);
            console.groupEnd();
            """);

        return output;
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
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%ckey{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{keyhole.String}'`, cssVariable, cssOperator, cssType, cssString);
                    console.groupEnd();
                    """);
                break;
            case FormatType.Integer:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%ckey{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
                    console.groupEnd();
                    """);
                break;
            case FormatType.EventHandler:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%ckey{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace);
                    console.groupEnd();
                    """);
                break;
            case FormatType.Attribute:
                output.AppendLine($"""
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%ckey{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" }`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace);
                    console.groupEnd();
                    """);
                break;
            case FormatType.Html:
                int start = keyhole.Integer!.Value;
                int length = (int)keyhole.Long!.Value;
                if (keyhole.Key != string.Empty)
                {
                    output.AppendLine($"""
                        console.group(`{$"[{index}]",-4}  {$"%ckey{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" } %cbuffer[{start}..{start + length - 1}]`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace, cssLink);
                        """);
                }
                else
                {
                    output.AppendLine($"""
                        console.group(`{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 { $"%c{{ %c{keyhole.String} %c}}" } %cbuffer[{start}..{start + length - 1}]`, cssVariable, cssOperator, cssType, cssBrace, cssDefault, cssBrace, cssLink);
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
                    console.groupCollapsed(`{$"[{index}]",-4}  {$"%ckey{keyhole.Key}%c: %c{keyhole.Type}",-28} 🔴 %c{keyhole.Integer}`, cssVariable, cssOperator, cssType, cssNumber);
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
}