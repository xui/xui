using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web4.Events.Subsets;

namespace Web4;

public record struct EventListener(
    Action<Event> Listener,
    string? Format = null
);

public class WindowBuilder(RouteGroupBuilder routeGroupBuilder)
{
    private readonly Dictionary<string, List<EventListener>> listeners = [];

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

    public WindowBuilder AddEventListener(string type, Action<Event> listener, string? format = null)
    {
        var item = new EventListener(listener, format);
        if (listeners.TryGetValue(type, out List<EventListener>? value))
            value.Add(item);
        else
            listeners.Add(type, [item]);
        return this;
    }

    public WindowBuilder AddEventListener(string type, Action<Event> listener) => AddEventListener(type, listener, null);
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

    public Action<Event>? OnAbort { set { if (value is not null) AddEventListener("abort", value); } }
    public Action<Event>? OnAfterPrint { set { if (value is not null) AddEventListener("afterprint", value); } }
    public Action<Event>? OnAnimationEnd { set { if (value is not null) AddEventListener("animationend", value); } }
    public Action<Event>? OnAnimationIteration { set { if (value is not null) AddEventListener("animationiteration", value); } }
    public Action<Event>? OnAnimationStart { set { if (value is not null) AddEventListener("animationstart", value); } }
    public Action<Event>? OnAppInstalled { set { if (value is not null) AddEventListener("appinstalled", value); } }
    public Action<Event>? OnAuxClick { set { if (value is not null) AddEventListener("auxclick", value); } }
    public Action<Event>? OnBeforeInput { set { if (value is not null) AddEventListener("beforeinput", value); } }
    public Action<Event>? OnBeforeInstallPrompt { set { if (value is not null) AddEventListener("beforeinstallprompt", value); } }
    public Action<Event>? OnBeforeMatch { set { if (value is not null) AddEventListener("beforeMmatch", value); } }
    public Action<Event>? OnBeforePrint { set { if (value is not null) AddEventListener("beforeprint", value); } }
    public Action<Event>? OnBeforeToggle { set { if (value is not null) AddEventListener("beforetoggle", value); } }
    public Action<Event>? OnBeforeXrSelect { set { if (value is not null) AddEventListener("beforexrselect", value); } }
    public Action<Event.Focus>? OnBlur { set { if (value is not null) AddEventListener("blur", value); } }
    public Action<Event>? OnCancel { set { if (value is not null) AddEventListener("cancel", value); } }
    public Action<Event>? OnCanPlay { set { if (value is not null) AddEventListener("canplay", value); } }
    public Action<Event>? OnCanPlayThrough { set { if (value is not null) AddEventListener("canplaythrough", value); } }
    public Action<Event>? OnChange { set { if (value is not null) AddEventListener("change", value); } }
    public Action<Event>? OnClick { set { if (value is not null) AddEventListener("click", value); } }
    public Action<Event>? OnClose { set { if (value is not null) AddEventListener("close", value); } }
    public Action<Event>? OnContentVisibilityAutoStateChange { set { if (value is not null) AddEventListener("contentvisibilityautostatechange", value); } }
    public Action<Event>? OnContextLost { set { if (value is not null) AddEventListener("contextlost", value); } }
    public Action<Event>? OnContextMenu { set { if (value is not null) AddEventListener("contextmenu", value); } }
    public Action<Event>? OnContextRestored { set { if (value is not null) AddEventListener("contextrestored", value); } }
    public Action<Event>? OnCueChange { set { if (value is not null) AddEventListener("cuechange", value); } }
    public Action<Event>? OnDblClick { set { if (value is not null) AddEventListener("dblclick", value); } }
    public Action<Event.DeviceMotion>? OnDeviceMotion { set { if (value is not null) AddEventListener("devicemotion", value); } }
    public Action<Event.DeviceOrientation>? OnDeviceOrientation { set { if (value is not null) AddEventListener("deviceorientation", value); } }
    public Action<Event.DeviceOrientation>? OnDeviceOrientationAbsolute { set { if (value is not null) AddEventListener("deviceorientationabsolute", value); } }
    public Action<Event>? OnDrag { set { if (value is not null) AddEventListener("drag", value); } }
    public Action<Event>? OnDragEnd { set { if (value is not null) AddEventListener("dragend", value); } }
    public Action<Event>? OnDragEnter { set { if (value is not null) AddEventListener("dragenter", value); } }
    public Action<Event>? OnDragLeave { set { if (value is not null) AddEventListener("dragleave", value); } }
    public Action<Event>? OnDragOver { set { if (value is not null) AddEventListener("dragover", value); } }
    public Action<Event>? OnDragStart { set { if (value is not null) AddEventListener("dragstart", value); } }
    public Action<Event>? OnDrop { set { if (value is not null) AddEventListener("drop", value); } }
    public Action<Event>? OnDurationChange { set { if (value is not null) AddEventListener("durationchange", value); } }
    public Action<Event>? OnEmptied { set { if (value is not null) AddEventListener("emptied", value); } }
    public Action<Event>? OnEnded { set { if (value is not null) AddEventListener("ended", value); } }
    public Action<Event.Error>? OnError { set { if (value is not null) AddEventListener("error", value); } }
    public Action<Event.Focus>? OnFocus { set { if (value is not null) AddEventListener("focus", value); } }
    public Action<Event>? OnFormdata { set { if (value is not null) AddEventListener("formdata", value); } }
    public Action<Event>? OnGotPointerCapture { set { if (value is not null) AddEventListener("gotpointercapture", value); } }
    public Action<Event.HashChange>? OnHashChange { set { if (value is not null) AddEventListener("hashchange", value); } }
    public Action<Event>? OnInput { set { if (value is not null) AddEventListener("input", value); } }
    public Action<Event>? OnInvalid { set { if (value is not null) AddEventListener("invalid", value); } }
    public Action<Event>? OnKeyDown { set { if (value is not null) AddEventListener("keydown", value); } }
    public Action<Event>? OnKeyPress { set { if (value is not null) AddEventListener("keypress", value); } }
    public Action<Event>? OnKeyUp { set { if (value is not null) AddEventListener("keyup", value); } }
    public Action<Event>? OnLanguageChange { set { if (value is not null) AddEventListener("languagechange", value); } }
    public Action<Event>? OnLoad { set { if (value is not null) AddEventListener("load", value); } }
    public Action<Event>? OnLoadedData { set { if (value is not null) AddEventListener("loadeddata", value); } }
    public Action<Event>? OnLoadedMetaData { set { if (value is not null) AddEventListener("loadedmetadata", value); } }
    public Action<Event>? OnLoadStart { set { if (value is not null) AddEventListener("loadstart", value); } }
    public Action<Event>? OnLostPointerCapture { set { if (value is not null) AddEventListener("lostpointercapture", value); } }
    public Action<Event>? OnMouseDown { set { if (value is not null) AddEventListener("mousedown", value); } }
    public Action<Event>? OnMouseEnter { set { if (value is not null) AddEventListener("mouseenter", value); } }
    public Action<Event>? OnMouseLeave { set { if (value is not null) AddEventListener("mouseleave", value); } }
    public Action<Event>? OnMouseMove { set { if (value is not null) AddEventListener("mousemove", value); } }
    public Action<Event>? OnMouseOut { set { if (value is not null) AddEventListener("mouseout", value); } }
    public Action<Event>? OnMouseOver { set { if (value is not null) AddEventListener("mouseover", value); } }
    public Action<Event>? OnMouseUp { set { if (value is not null) AddEventListener("mouseup", value); } }
    public Action<Event>? OnMouseWheel { set { if (value is not null) AddEventListener("mousewheel", value); } }
    public Action<Event.HashChange>? OnPageHide { set { if (value is not null) AddEventListener("pagehide", value); } }
    public Action<Event.HashChange>? OnPageReveal { set { if (value is not null) AddEventListener("pagereveal", value); } }
    public Action<Event.HashChange>? OnPageShow { set { if (value is not null) AddEventListener("pageshow", value); } }
    public Action<Event.HashChange>? OnPageSwap { set { if (value is not null) AddEventListener("pageswap", value); } }
    public Action<Event>? OnPause { set { if (value is not null) AddEventListener("pause", value); } }
    public Action<Event>? OnPlay { set { if (value is not null) AddEventListener("play", value); } }
    public Action<Event>? OnPlaying { set { if (value is not null) AddEventListener("playing", value); } }
    public Action<Event>? OnPointerCancel { set { if (value is not null) AddEventListener("pointercancel", value); } }
    public Action<Event>? OnPointerDown { set { if (value is not null) AddEventListener("pointerdown", value); } }
    public Action<Event>? OnPointerEnter { set { if (value is not null) AddEventListener("pointerenter", value); } }
    public Action<Event>? OnPointerLeave { set { if (value is not null) AddEventListener("pointerleave", value); } }
    public Action<Event>? OnPointerMove { set { if (value is not null) AddEventListener("pointermove", value); } }
    public Action<Event>? OnPointerOut { set { if (value is not null) AddEventListener("pointerout", value); } }
    public Action<Event>? OnPointerOver { set { if (value is not null) AddEventListener("pointerover", value); } }
    public Action<Event>? OnPointerRawUpdate { set { if (value is not null) AddEventListener("pointerrawupdate", value); } }
    public Action<Event>? OnPointerUp { set { if (value is not null) AddEventListener("pointerup", value); } }
    public Action<Event>? OnPopState { set { if (value is not null) AddEventListener("popstate", value); } }
    public Action<Event>? OnProgress { set { if (value is not null) AddEventListener("progress", value); } }
    public Action<Event>? OnRateChange { set { if (value is not null) AddEventListener("ratechange", value); } }
    public Action<Event>? OnRejectionHandled { set { if (value is not null) AddEventListener("rejectionhandled", value); } }
    public Action<Event>? OnReset { set { if (value is not null) AddEventListener("reset", value); } }
    public Action<Event>? OnResize { set { if (value is not null) AddEventListener("resize", value); } }
    public Action<Event>? OnScroll { set { if (value is not null) AddEventListener("scroll", value); } }
    public Action<Event>? OnScrollend { set { if (value is not null) AddEventListener("scrollend", value); } }
    public Action<Event>? OnScrollSnapChange { set { if (value is not null) AddEventListener("scrollsnapchange", value); } }
    public Action<Event>? OnScrollSnapChanging { set { if (value is not null) AddEventListener("scrollsnapchanging", value); } }
    public Action<Event>? OnSearch { set { if (value is not null) AddEventListener("search", value); } }
    public Action<Event>? OnSecurityPolicyViolation { set { if (value is not null) AddEventListener("securitypolicyviolation", value); } }
    public Action<Event>? OnSeeked { set { if (value is not null) AddEventListener("seeked", value); } }
    public Action<Event>? OnSeeking { set { if (value is not null) AddEventListener("seeking", value); } }
    public Action<Event>? OnSelect { set { if (value is not null) AddEventListener("select", value); } }
    public Action<Event>? OnSelectionChange { set { if (value is not null) AddEventListener("selectionchange", value); } }
    public Action<Event>? OnSelectStart { set { if (value is not null) AddEventListener("selectstart", value); } }
    public Action<Event>? OnSlotChange { set { if (value is not null) AddEventListener("slotchange", value); } }
    public Action<Event>? OnStalled { set { if (value is not null) AddEventListener("stalled", value); } }
    public Action<Event>? OnSubmit { set { if (value is not null) AddEventListener("submit", value); } }
    public Action<Event>? OnSuspend { set { if (value is not null) AddEventListener("suspend", value); } }
    public Action<Event>? OnGimeUpdate { set { if (value is not null) AddEventListener("timeupdate", value); } }
    public Action<Event>? OnToggle { set { if (value is not null) AddEventListener("toggle", value); } }
    public Action<Event>? OnTransitionCancel { set { if (value is not null) AddEventListener("transitioncancel", value); } }
    public Action<Event>? OnTransitionEnd { set { if (value is not null) AddEventListener("transitionend", value); } }
    public Action<Event>? OnTransitionRun { set { if (value is not null) AddEventListener("transitionrun", value); } }
    public Action<Event>? OnTransitionStart { set { if (value is not null) AddEventListener("transitionstart", value); } }
    public Action<Event>? OnUnhandledRejection { set { if (value is not null) AddEventListener("unhandledrejection", value); } }
    public Action<Event>? OnUnload { set { if (value is not null) AddEventListener("unload", value); } }
    public Action<Event>? OnVolumeChange { set { if (value is not null) AddEventListener("volumechange", value); } }
    public Action<Event>? OnWaiting { set { if (value is not null) AddEventListener("waiting", value); } }
    public Action<Event>? OnWheel { set { if (value is not null) AddEventListener("wheel", value); } }
}