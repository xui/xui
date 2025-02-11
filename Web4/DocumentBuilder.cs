using Web4.Events;
using Web4.EventListeners;

namespace Web4;

public class DocumentBuilder :
    IKeyboardEventListeners
{
    private readonly Dictionary<string, List<EventListener>> listeners = [];

    public Action<Aliases.Keyboard>? OnKeyDown { set => AddEventListener(nameof(OnKeyDown), value, true); }
    public Action<Aliases.Keyboard>? OnKeyUp { set => AddEventListener(nameof(OnKeyUp), value, true); }

    private DocumentBuilder AddEventListener(
        string type, 
        Action<Event>? listener, 
        bool isOnEvent = false,
        string? format = null)
    {
        listeners.TryGetValue(type, out var listenerSet);

        if (isOnEvent)
        {
            // Example: "OnClick" => "click"
            type = type[2..].ToLower();

            // Setting multiple listeners using OnEvents like `window.OnClick = ...` 
            // does not create multiple listeners, it replaces the pre-existing OnEvent, if any.
            listenerSet?.RemoveAll(l => l.IsOnEvent);
        }

        // This approach also ensures listeners are called in the proper order.  For example:
        //   document.OnClick = e => console.log("click1");
        //   document.AddEventListener("click", e => console.log("click2"));
        //   document.OnClick = e => null;
        //   document.OnClick = e => console.log("click3");
        //   document.AddEventListener("click", e => console.log("click4"));
        // Outputs:
        //   click2
        //   click3
        //   click4

        if (listener is not null)
        {
            var item = new EventListener(listener, format, isOnEvent);
            if (listenerSet is null)
            {
                listeners[type] = [item];
            }
            else
            {
                listenerSet.Add(item);
            }
        }

        return this;
    }
}