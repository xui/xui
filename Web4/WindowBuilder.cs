using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web4.Events;
using Web4.EventListeners;

namespace Web4;

public class WindowBuilder(RouteGroupBuilder routeGroupBuilder) : 
    IEventListeners,
    IAnimationEventListeners,
    IClipboardListeners,
    IDragEventListeners,
    IKeyboardEventListeners,
    ITransitionEventListeners
{
    private readonly Dictionary<string, List<EventListener>> listeners = [];

    public DocumentBuilder Document { get; } = new();

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

    public WindowBuilder MapGet(
        [StringSyntax("Route")] string pattern, 
        Action<HttpContext> requestDelegate)
    {
        routeGroupBuilder.Map(
            pattern,
            async context => 
            {
                // TODO: Implement
                await Task.Delay(1);
            }
        );

        return this;
    }

    public WindowBuilder AddEventListener(string type, Action<Event.Animation> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Composition> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.DeviceMotion> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.DeviceOrientation> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Drag> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Error> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Focus> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.HashChange> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<string>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<bool>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<int>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<long>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<float>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<double>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<decimal>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<DateTime>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<DateOnly>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<TimeOnly>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<Color>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<Uri>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Keyboard> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Mouse> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Pointer> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Progress> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Submit> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Touch> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Transition> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Wheel> listener) => AddEventListener(type, listener, null);

    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Angles> listener) => AddEventListener(type, listener, Event.Subsets.Angles.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Animation> listener) => AddEventListener(type, listener, Event.Subsets.Animation.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Buttons> listener) => AddEventListener(type, listener, Event.Subsets.Buttons.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ClientXY> listener) => AddEventListener(type, listener, Event.Subsets.ClientXY.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Coordinates> listener) => AddEventListener(type, listener, Event.Subsets.Coordinates.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Data> listener) => AddEventListener(type, listener, Event.Subsets.Data.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.DataTransfer> listener) => AddEventListener(type, listener, Event.Subsets.DataTransfer.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Deltas> listener) => AddEventListener(type, listener, Event.Subsets.Deltas.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Detail> listener) => AddEventListener(type, listener, Event.Subsets.Detail.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.DeviceMotion> listener) => AddEventListener(type, listener, Event.Subsets.DeviceMotion.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.DeviceOrientation> listener) => AddEventListener(type, listener, Event.Subsets.DeviceOrientation.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Error> listener) => AddEventListener(type, listener, Event.Subsets.Error.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.HashChange> listener) => AddEventListener(type, listener, Event.Subsets.HashChange.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.IsComposing> listener) => AddEventListener(type, listener, Event.Subsets.IsComposing.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Keys> listener) => AddEventListener(type, listener, Event.Subsets.Keys.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierAlt> listener) => AddEventListener(type, listener, Event.Subsets.ModifierAlt.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierCtrl> listener) => AddEventListener(type, listener, Event.Subsets.ModifierCtrl.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierMeta> listener) => AddEventListener(type, listener, Event.Subsets.ModifierMeta.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Modifiers> listener) => AddEventListener(type, listener, Event.Subsets.Modifiers.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierShift> listener) => AddEventListener(type, listener, Event.Subsets.ModifierShift.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.MovementXY> listener) => AddEventListener(type, listener, Event.Subsets.MovementXY.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.OffsetXY> listener) => AddEventListener(type, listener, Event.Subsets.OffsetXY.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.PageXY> listener) => AddEventListener(type, listener, Event.Subsets.PageXY.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Pointer> listener) => AddEventListener(type, listener, Event.Subsets.Pointer.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Pressures> listener) => AddEventListener(type, listener, Event.Subsets.Pressures.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Progress> listener) => AddEventListener(type, listener, Event.Subsets.Progress.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.RelatedTarget> listener) => AddEventListener(type, listener, Event.Subsets.RelatedTarget.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ScreenXY> listener) => AddEventListener(type, listener, Event.Subsets.ScreenXY.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Submitter> listener) => AddEventListener(type, listener, Event.Subsets.Submitter.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<string>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<bool>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<int>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<long>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<float>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<double>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<decimal>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<DateTime>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<DateOnly>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<TimeOnly>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<Color>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<Uri>> listener) => AddEventListener(type, listener, Event.Subsets.Target.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Tilts> listener) => AddEventListener(type, listener, Event.Subsets.Tilts.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Touches> listener) => AddEventListener(type, listener, Event.Subsets.Touches.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.WidthHeight> listener) => AddEventListener(type, listener, Event.Subsets.WidthHeight.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.X> listener) => AddEventListener(type, listener, Event.Subsets.X.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.XY> listener) => AddEventListener(type, listener, Event.Subsets.XY.Format);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Y> listener) => AddEventListener(type, listener, Event.Subsets.Y.Format);

    public WindowBuilder AddEventListener(string type, Action<Event>? listener, string? format = null)
        => AddEventListener(type, listener, false, format);

    private WindowBuilder AddEventListener(
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
        //   window.OnClick = e => console.log("click1");
        //   window.AddEventListener("click", e => console.log("click2"));
        //   window.OnClick = e => null;
        //   window.OnClick = e => console.log("click3");
        //   window.AddEventListener("click", e => console.log("click4"));
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