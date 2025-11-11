using System.Text;
using Web4.Core.DOM;

namespace Web4;

public ref struct KeyholeDumper(IConsole Console, Keyhole[] buffer)
{
    const string CSS_NOTES = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_DEFAULT = "font-weight:normal;font-family:monospace,monospace;";
    const string CSS_VARIABLE = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_NUMBER = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_STRING = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_TYPE = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_OPERATOR = "font-weight:normal;font-family:monospace,monospace;";
    const string CSS_LITERAL = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_LINK = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_BRACE = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_FUNCTION = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_HTML = "font-weight:normal;font-family:monospace,monospace;";

    public void Dump()
    {
        Console.Group("Remote Keyhole Buffer");

        var rootLength = buffer[0].SequenceLength;
        for (int index = 0; index < rootLength; index++)
        {
            ref Keyhole keyhole = ref buffer[index];
            WriteKeyhole(index, keyhole);
        }

        // var keyholeSize = sizeof(Keyhole);
        Console.Log($"%cRemote RAM:\n  buffer:     {170 * 40:n0} bytes\n  keyholes:   {85}\n    values:   {45}\n    pointers: {40}", CSS_NOTES);
        Console.GroupEnd();
    }

    private void WriteKeyhole(int index, Keyhole keyhole)
    {
                
        switch (keyhole.Type)
        {
            case KeyholeType.StringLiteral:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c ",-24} %c`{InlineString(keyhole.String)}`", CSS_VARIABLE, CSS_LITERAL);
                Console.Log($"\n{keyhole.String}\n\n");
                Console.GroupEnd();
                break;
            case KeyholeType.String:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} %o", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, $"globalThis.keyholes.{keyhole.Key}.element");
                Console.GroupEnd();
                break;
            case KeyholeType.Integer:
                if (keyhole.IsValueAnAttribute) // TODO: This isn't getting set?
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} %c{keyhole.Integer}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                else
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} %o", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, $"globalThis.keyholes.{keyhole.Key}.element");
                Console.GroupEnd();
                break;
            case KeyholeType.Boolean:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} %c{(keyhole.Boolean ? "true" : "false")}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                Console.GroupEnd();
                break;
            case KeyholeType.Color:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} %c◼ %o", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, $"color:#{keyhole.Color.ToRgb():x6}", $"globalThis.keyholes.{keyhole.Key}.element");
                Console.GroupEnd();
                break;
            // TODO: Support the other FormatTypes too
            case KeyholeType.EventListener:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} {$"%c{{ %c{keyhole.Expression} %c}}"}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                Console.GroupEnd();
                break;
            case KeyholeType.Attribute:
                int start = keyhole.SequenceStart;
                int length = keyhole.SequenceLength;
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} {$"%c{{ %c{keyhole.Expression?.Replace("  ", "").Replace("\n", " ")} %c}}"} %cbuffer[{keyhole.Sequence}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k);
                }
                Console.GroupEnd();
                break;
            case KeyholeType.Html:
                start = keyhole.SequenceStart;
                length = keyhole.SequenceLength;
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} {$"%c{{ %o %c}}"} %cbuffer[{keyhole.Sequence}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, $"globalThis.keyholes.{keyhole.Key}.element", CSS_BRACE, CSS_LINK);

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k);
                }
                Console.GroupEnd();
                break;
            case KeyholeType.Enumerable:
                start = keyhole.SequenceStart;
                length = keyhole.SequenceLength;
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} {$"%c({length} items)"} %cbuffer[{keyhole.Sequence}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_DEFAULT, CSS_LINK);

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k);
                }
                Console.GroupEnd();
                break;
            default:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} %c{keyhole.Double}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                Console.GroupEnd();
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
            ? inlined[..(maxLength - 3)] + "..."
            : inlined;
    }
}