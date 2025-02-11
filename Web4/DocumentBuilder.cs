using Web4.Events;
using Web4.EventListeners;

namespace Web4;

public class DocumentBuilder :
    IEventListeners,
    IAnimationEventListeners,
    IClipboardListeners,
    IDragEventListeners,
    IKeyboardEventListeners,
    ITransitionEventListeners
{
    private readonly Dictionary<string, List<EventListener>> listeners = [];

    public Action<Event>? OnBeforeInput { set => AddEventListener(nameof(OnBeforeInput), value, true); }
    public Action<Event>? OnContentVisibilityAutoStateChange { set => AddEventListener(nameof(OnContentVisibilityAutoStateChange), value, true); }
    public Action<Event>? OnInput { set => AddEventListener(nameof(OnInput), value, true); }
    public Action<Event>? OnSecurityPolicyViolation { set => AddEventListener(nameof(OnSecurityPolicyViolation), value, true); }

    public Action<Aliases.Animation>? OnAnimationCancel { set => AddEventListener(nameof(OnAnimationCancel), value, true); }
    public Action<Aliases.Animation>? OnAnimationEnd { set => AddEventListener(nameof(OnAnimationEnd), value, true); }
    public Action<Aliases.Animation>? OnAnimationIteration { set => AddEventListener(nameof(OnAnimationIteration), value, true); }
    public Action<Aliases.Animation>? OnAnimationStart { set => AddEventListener(nameof(OnAnimationStart), value, true); }

    public Action<Event>? OnCopy { set => AddEventListener(nameof(OnCopy), value, true); }
    public Action<Event>? OnCut { set => AddEventListener(nameof(OnCut), value, true); }
    public Action<Event>? OnPaste { set => AddEventListener(nameof(OnPaste), value, true); }

    public Action<Aliases.Drag>? OnDrag { set => AddEventListener(nameof(OnDrag), value, true); }
    public Action<Aliases.Drag>? OnDragEnd { set => AddEventListener(nameof(OnDragEnd), value, true); }
    public Action<Aliases.Drag>? OnDragEnter { set => AddEventListener(nameof(OnDragEnter), value, true); }
    public Action<Aliases.Drag>? OnDragLeave { set => AddEventListener(nameof(OnDragLeave), value, true); }
    public Action<Aliases.Drag>? OnDragOver { set => AddEventListener(nameof(OnDragOver), value, true); }
    public Action<Aliases.Drag>? OnDragStart { set => AddEventListener(nameof(OnDragStart), value, true); }
    public Action<Aliases.Drag>? OnDrop { set => AddEventListener(nameof(OnDrop), value, true); }
    
    public Action<Aliases.Keyboard>? OnKeyDown { set => AddEventListener(nameof(OnKeyDown), value, true); }
    public Action<Aliases.Keyboard>? OnKeyUp { set => AddEventListener(nameof(OnKeyUp), value, true); }

    public Action<Aliases.Transition>? OnTransitionCancel { set => AddEventListener(nameof(OnTransitionCancel), value, true); }
    public Action<Aliases.Transition>? OnTransitionEnd { set => AddEventListener(nameof(OnTransitionEnd), value, true); }
    public Action<Aliases.Transition>? OnTransitionRun { set => AddEventListener(nameof(OnTransitionRun), value, true); }
    public Action<Aliases.Transition>? OnTransitionStart { set => AddEventListener(nameof(OnTransitionStart), value, true); }

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