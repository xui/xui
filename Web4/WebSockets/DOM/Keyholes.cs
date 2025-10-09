using Web4.Core.DOM;
using Web4.JsonRpc;

namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
partial class WebSocketTransport : IKeyholes
{
    public void SetTextNode(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        Output.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setTextNode"),
            param: ref newKeyhole
        );
    }

    public void SetAttribute(ref Keyhole oldKeyhole, ref Keyhole newKeyhole)
    {
        Output.WriteNotification(
            method: ("app.keyholes", newKeyhole.Key, "setAttribute"),
            param: ref newKeyhole
        );
    }

    public void SetAttribute(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes)
    {
        Output.WriteNotification(
            method: ("app.keyholes", key, "setAttribute"),
            newKeyholes,
            includeSentinels: false
        );
    }

    public void SetElement(string key, Span<Keyhole> oldKeyholes, Span<Keyhole> newKeyholes, string? transition)
    {
        Output.WriteNotification(
            method: ("app.keyholes", key, "setElement"),
            newKeyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    public void AddElement(string priorKey, string key, Span<Keyhole> keyholes, string? transition)
    {
        Output.WriteNotification(
            method: ("app.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            includeSentinels: true,
            transition: transition
        );
    }

    public void RemoveElement(string key, string? transition)
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

    public void Dump()
    {
        var buffer = CaptureSnapshot();
        new KeyholeDumper(Console, buffer).Dump();
    }
}
