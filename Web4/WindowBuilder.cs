using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web4.Events;
using Web4.EventListeners;

namespace Web4;

public class WindowBuilder(RouteGroupBuilder routeGroupBuilder) : 
    IWindowEventListeners,
    IEventListeners,
    IAnimationEventListeners,
    IClipboardListeners,
    IContentVisibilityEventListeners,
    IDragEventListeners,
    IFocusEventListeners,
    IFormDataEventListeners,
    IKeyboardEventListeners,
    IMouseEventListeners,
    IPointerEventListeners,
    IScrollEventListeners,
    ISubmitEventListeners,
    IToggleEventListeners,
    ITouchEventListeners,
    ITransitionEventListeners
{
    private readonly Dictionary<string, List<EventListener>> listeners = [];

    public DocumentBuilder Document { get; } = new();

    public Action<Event>? OnAfterPrint { set => AddEventListener(nameof(OnAfterPrint), value, true); }
    public Action<Event>? OnBeforePrint { set => AddEventListener(nameof(OnBeforePrint), value, true); }
    public Action<Event.BeforeUnload>? OnBeforeUnload { set => AddEventListener(nameof(OnBeforeUnload), value, true); }
    public Action<Event.DeviceMotion>? OnDeviceMotion { set => AddEventListener(nameof(OnDeviceMotion), value, true); }
    public Action<Event.DeviceOrientation>? OnDeviceOrientation { set => AddEventListener(nameof(OnDeviceOrientation), value, true); }
    public Action<Event.DeviceOrientation>? OnDeviceOrientationAbsolute { set => AddEventListener(nameof(OnDeviceOrientationAbsolute), value, true); }
    public Action<Event.Gamepad>? OnGamepadConnected { set => AddEventListener(nameof(OnGamepadConnected), value, true); }
    public Action<Event.Gamepad>? OnGamepadDisconnected { set => AddEventListener(nameof(OnGamepadDisconnected), value, true); }
    public Action<Event.HashChange>? OnHashChange { set => AddEventListener(nameof(OnHashChange), value, true); }
    public Action<Event>? OnLanguageChange { set => AddEventListener(nameof(OnLanguageChange), value, true); }
    public Action<Event.Message>? OnMessage { set => AddEventListener(nameof(OnMessage), value, true); }
    public Action<Event.Message>? OnMessageError { set => AddEventListener(nameof(OnMessageError), value, true); }
    public Action<Event>? OnOffline { set => AddEventListener(nameof(OnOffline), value, true); }
    public Action<Event>? OnOnline { set => AddEventListener(nameof(OnOnline), value, true); }
    public Action<Event.PageTransition>? OnPageHide { set => AddEventListener(nameof(OnPageHide), value, true); }
    public Action<Event.PageTransition>? OnPageShow { set => AddEventListener(nameof(OnPageShow), value, true); }
    public Action<Event.PopState>? OnPopState { set => AddEventListener(nameof(OnPopState), value, true); }
    public Action<Event.PromiseRejection>? OnRejectionHandled { set => AddEventListener(nameof(OnRejectionHandled), value, true); }
    public Action<Event>? OnResize { set => AddEventListener(nameof(OnResize), value, true); }
    public Action<Event.Storage>? OnStorage { set => AddEventListener(nameof(OnStorage), value, true); }
    public Action<Event.PromiseRejection>? OnUnhandledRejection { set => AddEventListener(nameof(OnUnhandledRejection), value, true); }

    public Action<Event>? OnBeforeInput { set => AddEventListener(nameof(OnBeforeInput), value, true); }
    public Action<Event>? OnCancel { set => AddEventListener(nameof(OnCancel), value, true); }
    public Action<Event>? OnChange { set => AddEventListener(nameof(OnChange), value, true); }
    public Action<Event>? OnCueChange { set => AddEventListener(nameof(OnCueChange), value, true); }
    public Action<Event>? OnInput { set => AddEventListener(nameof(OnInput), value, true); }
    public Action<Event>? OnInvalid { set => AddEventListener(nameof(OnInvalid), value, true); }
    public Action<Event>? OnLoad { set => AddEventListener(nameof(OnLoad), value, true); }
    public Action<Event>? OnReset { set => AddEventListener(nameof(OnReset), value, true); }
    public Action<Event>? OnSelect { set => AddEventListener(nameof(OnSelect), value, true); }
    public Action<Event>? OnSecurityPolicyViolation { set => AddEventListener(nameof(OnSecurityPolicyViolation), value, true); }
    public Action<Event>? OnSelectStart { set => AddEventListener(nameof(OnSelectStart), value, true); }
    public Action<Event>? OnSlotChange { set => AddEventListener(nameof(OnSlotChange), value, true); }

    public Action<Event.Animation>? OnAnimationCancel { set => AddEventListener(nameof(OnAnimationCancel), value, true); }
    public Action<Event.Animation>? OnAnimationEnd { set => AddEventListener(nameof(OnAnimationEnd), value, true); }
    public Action<Event.Animation>? OnAnimationIteration { set => AddEventListener(nameof(OnAnimationIteration), value, true); }
    public Action<Event.Animation>? OnAnimationStart { set => AddEventListener(nameof(OnAnimationStart), value, true); }

    public Action<Event.Clipboard>? OnCopy { set => AddEventListener(nameof(OnCopy), value, true); }
    public Action<Event.Clipboard>? OnCut { set => AddEventListener(nameof(OnCut), value, true); }
    public Action<Event.Clipboard>? OnPaste { set => AddEventListener(nameof(OnPaste), value, true); }

    public Action<Event.ContentVisibilityAutoStateChange>? OnContentVisibilityAutoStateChange { set => AddEventListener(nameof(OnContentVisibilityAutoStateChange), value, true); }

    public Action<Event.Drag>? OnDrag { set => AddEventListener(nameof(OnDrag), value, true); }
    public Action<Event.Drag>? OnDragEnd { set => AddEventListener(nameof(OnDragEnd), value, true); }
    public Action<Event.Drag>? OnDragEnter { set => AddEventListener(nameof(OnDragEnter), value, true); }
    public Action<Event.Drag>? OnDragLeave { set => AddEventListener(nameof(OnDragLeave), value, true); }
    public Action<Event.Drag>? OnDragOver { set => AddEventListener(nameof(OnDragOver), value, true); }
    public Action<Event.Drag>? OnDragStart { set => AddEventListener(nameof(OnDragStart), value, true); }
    public Action<Event.Drag>? OnDrop { set => AddEventListener(nameof(OnDrop), value, true); }

    public Action<Event.Focus>? OnBlur { set => AddEventListener(nameof(OnBlur), value, true); }
    public Action<Event.Focus>? OnFocus { set => AddEventListener(nameof(OnFocus), value, true); }
    public Action<Event.Focus>? OnFocusIn { set => AddEventListener(nameof(OnFocusIn), value, true); }
    public Action<Event.Focus>? OnFocusOut { set => AddEventListener(nameof(OnFocusOut), value, true); }

    public Action<Event.FormData>? OnFormData { set => AddEventListener(nameof(OnFormData), value, true); }

    public Action<Event.Keyboard>? OnKeyDown { set => AddEventListener(nameof(OnKeyDown), value, true); }
    public Action<Event.Keyboard>? OnKeyUp { set => AddEventListener(nameof(OnKeyUp), value, true); }

    public Action<Event.Mouse>? OnAuxClick { set => AddEventListener(nameof(OnAuxClick), value, true); }
    public Action<Event.Mouse>? OnClick { set => AddEventListener(nameof(OnClick), value, true); }
    public Action<Event.Mouse>? OnContextMenu { set => AddEventListener(nameof(OnContextMenu), value, true); }
    public Action<Event.Mouse>? OnDblClick { set => AddEventListener(nameof(OnDblClick), value, true); }
    public Action<Event.Mouse>? OnMouseDown { set => AddEventListener(nameof(OnMouseDown), value, true); }
    public Action<Event.Mouse>? OnMouseEnter { set => AddEventListener(nameof(OnMouseEnter), value, true); }
    public Action<Event.Mouse>? OnMouseLeave { set => AddEventListener(nameof(OnMouseLeave), value, true); }
    public Action<Event.Mouse>? OnMouseMove { set => AddEventListener(nameof(OnMouseMove), value, true); }
    public Action<Event.Mouse>? OnMouseOut { set => AddEventListener(nameof(OnMouseOut), value, true); }
    public Action<Event.Mouse>? OnMouseOver { set => AddEventListener(nameof(OnMouseOver), value, true); }
    public Action<Event.Mouse>? OnMouseUp { set => AddEventListener(nameof(OnMouseUp), value, true); }

    public Action<Event.Pointer>? OnGotPointerCapture { set => AddEventListener(nameof(OnGotPointerCapture), value, true); }
    public Action<Event.Pointer>? OnLostPointerCapture { set => AddEventListener(nameof(OnLostPointerCapture), value, true); }
    public Action<Event.Pointer>? OnPointerCancel { set => AddEventListener(nameof(OnPointerCancel), value, true); }
    public Action<Event.Pointer>? OnPointerDown { set => AddEventListener(nameof(OnPointerDown), value, true); }
    public Action<Event.Pointer>? OnPointerEnter { set => AddEventListener(nameof(OnPointerEnter), value, true); }
    public Action<Event.Pointer>? OnPointerLeave { set => AddEventListener(nameof(OnPointerLeave), value, true); }
    public Action<Event.Pointer>? OnPointerMove { set => AddEventListener(nameof(OnPointerMove), value, true); }
    public Action<Event.Pointer>? OnPointerOut { set => AddEventListener(nameof(OnPointerOut), value, true); }
    public Action<Event.Pointer>? OnPointerOver { set => AddEventListener(nameof(OnPointerOver), value, true); }
    public Action<Event.Pointer>? OnPointerUp { set => AddEventListener(nameof(OnPointerUp), value, true); }

    public Action<Event>? OnScroll { set => AddEventListener(nameof(OnScroll), value, true); }
    public Action<Event>? OnScrollEnd { set => AddEventListener(nameof(OnScrollEnd), value, true); }

    public Action<Event.Submit>? OnSubmit { set => AddEventListener(nameof(OnSubmit), value, true); }

    public Action<Event.Toggle>? OnBeforeToggle { set => AddEventListener(nameof(OnBeforeToggle), value, true); }
    public Action<Event.Toggle>? OnToggle { set => AddEventListener(nameof(OnToggle), value, true); }

    public Action<Event.Touch>? OnTouchCancel { set => AddEventListener(nameof(OnTouchCancel), value, true); }
    public Action<Event.Touch>? OnTouchEnd { set => AddEventListener(nameof(OnTouchEnd), value, true); }
    public Action<Event.Touch>? OnTouchMove { set => AddEventListener(nameof(OnTouchMove), value, true); }
    public Action<Event.Touch>? OnTouchStart { set => AddEventListener(nameof(OnTouchStart), value, true); }

    public Action<Event.Transition>? OnTransitionCancel { set => AddEventListener(nameof(OnTransitionCancel), value, true); }
    public Action<Event.Transition>? OnTransitionEnd { set => AddEventListener(nameof(OnTransitionEnd), value, true); }
    public Action<Event.Transition>? OnTransitionRun { set => AddEventListener(nameof(OnTransitionRun), value, true); }
    public Action<Event.Transition>? OnTransitionStart { set => AddEventListener(nameof(OnTransitionStart), value, true); }

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

    public WindowBuilder AddEventListener(string type, Action listener) => AddEventListener(type, e => listener(), string.Empty);
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