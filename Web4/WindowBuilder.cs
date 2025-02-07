using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Web4.Events.Subsets;

namespace Web4;

public record class EventListener(
    Action<Event> Listener,
    string? Format = null
);

public class WindowBuilder(RouteGroupBuilder routeGroupBuilder)
{
    private readonly Dictionary<string, List<EventListener>> listeners = [];

    public WindowBuilder AddEventListener(
        string type, Action<Event> listener, 
        string? format = null)
    {
        if (!listeners.ContainsKey(type))
            listeners.Add(type, []);
        listeners[type].Add(new (listener, format));
        return this;
    }

    public WindowBuilder AddEventListener(
        string type, 
        Action<Event.Subsets.ClientXY> listener) => 
            AddEventListener(type, listener, Event.Subsets.ClientXY.Format);

    public WindowBuilder AddEventListener(
        string type, 
        Action<Event.Subsets.XY> listener) => 
            AddEventListener(type, listener, Event.Subsets.XY.Format);

    public WindowBuilder AddEventListener(
        string type, 
        Action<Event> listener) => 
            AddEventListener(type, listener, null);
    
    public WindowBuilder MapGet(
        [StringSyntax("Route")] string pattern, 
        Action<HttpContext> requestDelegate)
    {
        routeGroupBuilder.Map(
            pattern,
            async context => 
            {
                await Task.Delay(1);
            }
        );

        return this;
    }

    // TODO: Finish...
    public Action<Event>? OnAbort { set { if (value != null) AddEventListener("abort", value); } }
    public Action<Event>? OnAfterPrint { get; set; }
    public Action<Event>? OnAnimationEnd { get; set; }
    public Action<Event>? OnAnimationIteration { get; set; }
    public Action<Event>? OnAnimationStart { get; set; }
    public Action<Event>? OnAppInstalled { get; set; }
    public Action<Event>? OnAuxClick { get; set; }
    public Action<Event>? OnBeforeInput { get; set; }
    public Action<Event>? OnBeforeInstallPrompt { get; set; }
    public Action<Event>? OnBeforeMatch { get; set; }
    public Action<Event>? OnBeforePrint { get; set; }
    public Action<Event>? OnBeforeToggle { get; set; }
    public Action<Event>? OnBeforeUnload { get; set; }
    public Action<Event>? OnBeforeXrSelect { get; set; }
    public Action<Event>? OnBlur { get; set; }
    public Action<Event>? OnCancel { get; set; }
    public Action<Event>? OnCanPlay { get; set; }
    public Action<Event>? Oncanplaythrough { get; set; }
    public Action<Event>? Onchange { get; set; }
    public Action<Event>? Onclick { get; set; }
    public Action<Event>? Onclose { get; set; }
    public Action<Event>? Oncontentvisibilityautostatechange { get; set; }
    public Action<Event>? Oncontextlost { get; set; }
    public Action<Event>? Oncontextmenu { get; set; }
    public Action<Event>? Oncontextrestored { get; set; }
    public Action<Event>? Oncuechange { get; set; }
    public Action<Event>? Ondblclick { get; set; }
    public Action<Event>? Ondevicemotion { get; set; }
    public Action<Event>? Ondeviceorientation { get; set; }
    public Action<Event>? Ondeviceorientationabsolute { get; set; }
    public Action<Event>? Ondrag { get; set; }
    public Action<Event>? Ondragend { get; set; }
    public Action<Event>? Ondragenter { get; set; }
    public Action<Event>? Ondragleave { get; set; }
    public Action<Event>? Ondragover { get; set; }
    public Action<Event>? Ondragstart { get; set; }
    public Action<Event>? Ondrop { get; set; }
    public Action<Event>? Ondurationchange { get; set; }
    public Action<Event>? Onemptied { get; set; }
    public Action<Event>? Onended { get; set; }
    public Action<Event>? Onerror { get; set; }
    public Action<Event>? Onfocus { get; set; }
    public Action<Event>? Onformdata { get; set; }
    public Action<Event>? Ongotpointercapture { get; set; }
    public Action<Event>? Onhashchange { get; set; }
    public Action<Event>? Oninput { get; set; }
    public Action<Event>? Oninvalid { get; set; }
    public Action<Event>? Onkeydown { get; set; }
    public Action<Event>? Onkeypress { get; set; }
    public Action<Event>? Onkeyup { get; set; }
    public Action<Event>? Onlanguagechange { get; set; }
    public Action<Event>? Onload { get; set; }
    public Action<Event>? Onloadeddata { get; set; }
    public Action<Event>? Onloadedmetadata { get; set; }
    public Action<Event>? Onloadstart { get; set; }
    public Action<Event>? Onlostpointercapture { get; set; }
    public Action<Event>? Onmessage { get; set; }
    public Action<Event>? Onmessageerror { get; set; }
    public Action<Event>? Onmousedown { get; set; }
    public Action<Event>? Onmouseenter { get; set; }
    public Action<Event>? Onmouseleave { get; set; }
    public Action<Event>? Onmousemove { get; set; }
    public Action<Event>? Onmouseout { get; set; }
    public Action<Event>? Onmouseover { get; set; }
    public Action<Event>? Onmouseup { get; set; }
    public Action<Event>? Onmousewheel { get; set; }
    public Action<Event>? Onoffline { get; set; }
    public Action<Event>? Ononline { get; set; }
    public Action<Event>? Onpagehide { get; set; }
    public Action<Event>? Onpagereveal { get; set; }
    public Action<Event>? Onpageshow { get; set; }
    public Action<Event>? Onpageswap { get; set; }
    public Action<Event>? Onpause { get; set; }
    public Action<Event>? Onplay { get; set; }
    public Action<Event>? Onplaying { get; set; }
    public Action<Event>? Onpointercancel { get; set; }
    public Action<Event>? Onpointerdown { get; set; }
    public Action<Event>? Onpointerenter { get; set; }
    public Action<Event>? Onpointerleave { get; set; }
    public Action<Event>? Onpointermove { get; set; }
    public Action<Event>? Onpointerout { get; set; }
    public Action<Event>? Onpointerover { get; set; }
    public Action<Event>? Onpointerrawupdate { get; set; }
    public Action<Event>? Onpointerup { get; set; }
    public Action<Event>? Onpopstate { get; set; }
    public Action<Event>? Onprogress { get; set; }
    public Action<Event>? Onratechange { get; set; }
    public Action<Event>? Onrejectionhandled { get; set; }
    public Action<Event>? Onreset { get; set; }
    public Action<Event>? Onresize { get; set; }
    public Action<Event>? Onscroll { get; set; }
    public Action<Event>? Onscrollend { get; set; }
    public Action<Event>? Onscrollsnapchange { get; set; }
    public Action<Event>? Onscrollsnapchanging { get; set; }
    public Action<Event>? Onsearch { get; set; }
    public Action<Event>? Onsecuritypolicyviolation { get; set; }
    public Action<Event>? Onseeked { get; set; }
    public Action<Event>? Onseeking { get; set; }
    public Action<Event>? Onselect { get; set; }
    public Action<Event>? Onselectionchange { get; set; }
    public Action<Event>? Onselectstart { get; set; }
    public Action<Event>? Onslotchange { get; set; }
    public Action<Event>? Onstalled { get; set; }
    public Action<Event>? Onstorage { get; set; }
    public Action<Event>? Onsubmit { get; set; }
    public Action<Event>? Onsuspend { get; set; }
    public Action<Event>? Ontimeupdate { get; set; }
    public Action<Event>? Ontoggle { get; set; }
    public Action<Event>? Ontransitioncancel { get; set; }
    public Action<Event>? Ontransitionend { get; set; }
    public Action<Event>? Ontransitionrun { get; set; }
    public Action<Event>? Ontransitionstart { get; set; }
    public Action<Event>? Onunhandledrejection { get; set; }
    public Action<Event>? Onunload { get; set; }
    public Action<Event>? Onvolumechange { get; set; }
    public Action<Event>? Onwaiting { get; set; }
    public Action<Event>? Onwebkitanimationend { get; set; }
    public Action<Event>? Onwebkitanimationiteration { get; set; }
    public Action<Event>? Onwebkitanimationstart { get; set; }
    public Action<Event>? Onwebkittransitionend { get; set; }
    public Action<Event>? Onwheel { get; set; }
}