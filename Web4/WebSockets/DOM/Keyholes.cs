namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
partial class WebSocketTransport : IKeyholes
{
    public void SetTextNode(string key, ref Keyhole keyhole)
    {
        Output.WriteNotification(
            method: ("ui.keyholes", key, "setTextNode"),
            param1: ref keyhole
        );
    }

    public void SetAttribute(string key, ref Keyhole keyhole)
    {
        Output.WriteNotification(
            method: ("ui.keyholes", key, "setAttribute"),
            param1: ref keyhole
        );
    }

    public void SetAttribute(string key, Span<Keyhole> keyholes)
    {
        Output.WriteNotification(
            method: ("ui.keyholes", key, "setAttribute"),
            param1: keyholes
        );
    }

    public void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes)
    {
        Output.WriteNotification(buffer,
            method: ("ui.keyholes", key, "setElement"),
            param1: keyholes
        );
    }

    public void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, bool reverseTransition)
    {
        Output.WriteNotification(buffer,
            method: ("ui.keyholes", key, "setElement"),
            param1: keyholes,
            param2: reverseTransition ? ("web4-rev-", key) : ("web4-fwd-", key)
        );
    }

    public void SetElement(Keyhole[] buffer, string key, Span<Keyhole> keyholes, object oldTag, object newTag)
    {
        Output.WriteNotification(buffer,
            method: ("ui.keyholes", key, "setElement"),
            param1: keyholes,
            param2: ("web4-move-", oldTag.GetHashCode()),
            param3: ("web4-move-", newTag.GetHashCode())
          );
    }

    public void AddElement(Keyhole[] buffer, string priorKey, string key, Span<Keyhole> keyholes, string? transition)
    {
        Output.WriteNotification(buffer,
            method: ("ui.keyholes", priorKey, "addElement"),
            param1: key,
            param2: keyholes,
            param3: transition
        );
    }

    public void RemoveElement(string key, string? transition)
    {
        if (transition is null)
            Output.WriteNotification(
                method: ("ui.keyholes", key, "removeElement")
            );
        else
            Output.WriteNotification(
                method: ("ui.keyholes", key, "removeElement"),
                param1: transition
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
