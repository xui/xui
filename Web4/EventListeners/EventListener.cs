namespace Web4.EventListeners;

internal record struct EventListener(
    Action<Event> Listener,
    string? Format = null,
    bool IsOnEvent = false
);

