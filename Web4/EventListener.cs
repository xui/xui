namespace Web4;

public record struct EventListener(
    Action? Action = null,
    Action<Event>? ActionEvent = null,
    Func<Task>? Func = null,
    Func<Event, Task>? FuncEvent = null,
    string? OnNotation = null,
    string? Html = null)
{
    public EventListener(Action listener) : this(Action: listener) { }
    public EventListener(Action<Event> listener) : this(ActionEvent: listener) { }
    public EventListener(Func<Task> listener) : this(Func: listener) { }
    public EventListener(Func<Event, Task> listener) : this(FuncEvent: listener) { }

    public readonly void Invoke(Event @event, int propagationID)
    {
        using var perf = Debug.PerfCheck("Invoke"); // TODO: Remove PerfCheck

        if (Action is not null)
        {
            Action();
            @event.Dispose();
        }
        else if (ActionEvent is not null)
        {
            // TODO: Boxing allocation: conversion from 'T' to 'Event' requires boxing of the value type
            ActionEvent(@event);
            @event.Dispose();
        }
        else if (Func is not null)
        {
            _ = Func();
            @event.Dispose();
        }
        else if (FuncEvent is not null)
        {
            // TODO: Boxing allocation: conversion from 'T' to 'Event' requires boxing of the value type
            _ = FuncEvent(@event)
                .ContinueWith(t => @event.Dispose());
            // TODO: This captures and therefore allocates
        }
        else
        {
            // TODO: Possible race condition: as a component gets unloaded
            // messages might pass each other across the network.
            Console.WriteLine("🔴 No event listener to invoke.  You need to investigate this.");
        }
    }
}