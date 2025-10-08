using System.Text;
using Web4.Core.DOM;

namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
public partial class WebSocketTransport : IKeyholes
{
    void IKeyholes.SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        Output.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setTextNode"),
            param: ref newKeyhole
        );
    }

    void IKeyholes.SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        Output.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setAttribute"),
            param: ref newKeyhole
        );
    }

    void IKeyholes.SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        Output.WriteNotification(
            method: ("app.keyholes", key, "setAttribute"),
            newKeyholes,
            includeSentinels: false
        );
    }

    void IKeyholes.SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition)
    {
        Output.WriteNotification(
            method: ("app.keyholes", key, "setElement"),
            newKeyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    void IKeyholes.AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition)
    {
        Output.WriteNotification(
            method: ("app.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    void IKeyholes.RemoveElement(string key, string? transition)
    {
        if (transition is null)
            Output.WriteNotification(
                method: ("app.keyholes", key, "removeElement")
            );
        else
            Output.WriteNotification(
                method: ("app.keyholes", key, "removeElement"),
                param: transition
            );
    }

    public void DispatchEvent(Action listener)
    {
        try
        {
            listener();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception from event listener:\n{ex}");
            Console.Error(ex);
        }
    }

    public void DispatchEvent<T>(Action<Event> listener, T @event)
        where T : struct, Event
    {
        try
        {
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            listener(@event);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception from event listener:\n{ex}");
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
        }
    }

    public async Task DispatchEvent(Func<Task> listener)
    {
        try
        {
            await listener();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception from event listener:\n{ex}");
            Console.Error(ex);
        }
    }

    public async Task DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct, Event
    {
        try
        {
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            await listener(@event);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Exception from event listener:\n{ex}");
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
        }
    }

    void IKeyholes.Dump()
    {
        const string CSS_NOTES = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";

        var buffer = App.CaptureSnapshot();

        Console.Group("Remote Keyhole Buffer");

        var rootLength = buffer[0].SequenceLength;
        for (int index = 0; index < rootLength; index++)
        {
            ref Keyhole keyhole = ref buffer[index];
            WriteKeyhole(index, keyhole, buffer);
        }

        // var keyholeSize = sizeof(Keyhole);
        Console.Log($"%cRemote RAM:\n  buffer:     {170 * 40:n0} bytes\n  keyholes:   {85}\n    values:   {45}\n    pointers: {40}", CSS_NOTES);
        Console.GroupEnd();
    }

    private void WriteKeyhole(int index, Keyhole keyhole, Keyhole[] buffer)
    {
        const string CSS_DEFAULT = "font-weight:normal;font-family:monospace,monospace;";
        const string CSS_VARIABLE = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
        const string CSS_NUMBER = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
        const string CSS_STRING = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
        const string CSS_TYPE = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
        const string CSS_OPERATOR = "font-weight:normal;font-family:monospace,monospace;";
        const string CSS_LITERAL = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
        const string CSS_LINK = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
        const string CSS_BRACE = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";
        // const string CSS_FUNCTION = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
        // const string CSS_HTML = "font-weight:normal;font-family:monospace,monospace;";
        // const string CSS_NOTES = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
                
        switch (keyhole.Type)
        {
            case KeyholeType.StringLiteral:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c ",-24} 🟢 %c`{InlineString(keyhole.String)}`", CSS_VARIABLE, CSS_LITERAL);
                Console.Log($"\n{keyhole.String}\n\n");
                Console.GroupEnd();
                break;
            case KeyholeType.String:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c'{InlineString(keyhole.String)}'", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_STRING);
                Console.GroupEnd();
                break;
            case KeyholeType.Integer:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Integer}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                Console.GroupEnd();
                break;
            case KeyholeType.Boolean:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{(keyhole.Boolean ? "true" : "false")}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                Console.GroupEnd();
                break;
            case KeyholeType.Color:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c◼ %c#{keyhole.Color.ToRgb():x6}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, $"color:#{keyhole.Color.ToRgb():x6}", CSS_NUMBER);
                Console.GroupEnd();
                break;
            // TODO: Support the other FormatTypes too
            case KeyholeType.EventListener:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 {$"%c{{ %c{keyhole.Expression} %c}}"}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                Console.GroupEnd();
                break;
            case KeyholeType.Attribute:
            case KeyholeType.Html:
            case KeyholeType.Enumerable:
                int start = keyhole.SequenceStart;
                int length = keyhole.SequenceLength;
                if (keyhole.Key != string.Empty)
                {
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 {$"%c{{ %c{keyhole.Expression?.Replace("  ", "").Replace("\n", " ")} %c}}"} %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);
                }
                else
                {
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c%c%c",-28} 🟢 {$"%c{{ %c{keyhole.Expression} %c}}"} %cbuffer[{start}..{start + length - 1}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE, CSS_LINK);
                }

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k, buffer);
                }
                Console.GroupEnd();
                break;
            default:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c{keyhole.Key}%c: %c{keyhole.Type}",-28} 🟢 %c{keyhole.Double}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
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
