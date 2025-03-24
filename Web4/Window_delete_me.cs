// using System.Security.Claims;
// using Microsoft.AspNetCore.Http;
// using Web4.Events;
// using Web4.Events.Subsets;

// namespace Web4;

// public static class EventExtensions
// {
//     public static Context GetContext(this IEvent e)
//     {
//         return new();
//     }

//     public static Context GetContext(this ISubset e)
//     {
//         return new();
//     }
// }

// public class BrowserConsole
// {
//     public void Log(string message) { }
// }

// public struct Context
// {
//     public Window Window { get; }
//     public HttpRequest Request { get; }
//     public HttpResponse Response { get; }
//     public ConnectionInfo Connection { get; }
//     public ISession Session { get; }
//     public ClaimsPrincipal User { get; set; }
// }

// public class Window
// {
//     // From Document?  Maybe make a tiny Document struct?
//     public string? Title { set { } }

//     // Objects
//     public BrowserConsole Console { get; } = new();

//     public float DevicePixelRatio { get; } = 2;
//     public int InnerWidth { get; } = 786;
//     public int InnerHeight { get; } = 577;
//     public int OuterWidth { get; } = 1666;
//     public int OuterHeight { get; } = 664;
//     public bool IsSecureContext { get; } = true;
//     public string Name { get; } = "";
//     public string Origin { get; } = "http://localhost:5003";
//     public object Screen { get; } = new();
//     public int ScreenLeft { get; } = 0;
//     public int ScreenTop { get; } = 416;
//     public float ScrollX { get; } = 0;
//     public float ScrollY { get; } = 783.5f;
//     public object VisualViewport { get; } = new();

//     // I'm not so sure about these...
//     public bool locationbar { get; } = true;
//     public bool menubar { get; } = true;
//     public bool personalbar { get; } = true;
//     public bool scrollbars { get; } = true;
//     public bool statusbar { get; } = true;
//     public bool toolbar { get; } = true;
//     //opener
//     //parent: Window
//     //top



//     public async Task Alert(string message) { await Task.Delay(1); }
//     public void Close() { }
//     public void Confirm() { }
//     public void Focus() { }
//     public void GetComputedStyle() { }
//     public void GetSelection() { }
//     public void MatchMedia() { }
//     public void MoveBy() { }
//     public void MoveTo() { }
//     public void Open() { }
//     public void PostMessage() { }
//     public void Print() { }
//     public void Prompt() { }
//     public void ResizeBy() { }
//     public void ResizeTo() { }
//     public void Scroll() { }
//     public void ScrollBy() { }
//     public void ScrollTo() { }
// }