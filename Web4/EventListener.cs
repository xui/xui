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

    public async readonly ValueTask Invoke(Event? e)
    {
        if (Action is not null)
        {
            Action();
        }
        else if (ActionEvent is not null && e is not null)
        {
            ActionEvent(e);
        }
        else if (Func is not null)
        {
            await Func();
        }
        else if (FuncEvent is not null && e is not null)
        {
            await FuncEvent(e);
        }
        else
        {
            Console.WriteLine("🔴 No event listener to invoke.  You need to investigate this.");
        }
    }
}