namespace Web4.Proxies;

public struct WindowProxy(IWeb4Transport transport)
{
    public readonly WindowProxy Window { get => this; }
    public readonly DocumentProxy Document { get => new(transport); }
    public readonly ConsoleProxy Console { get => new(transport); }

    public float DevicePixelRatio { get; } = 2;
    public int InnerWidth { get; } = 786;
    public int InnerHeight { get; } = 577;
    public int OuterWidth { get; } = 1666;
    public int OuterHeight { get; } = 664;
    public bool IsSecureContext { get; } = true;
    public string Name { get; } = "";
    public string Origin { get; } = "http://localhost:5003";
    public object Screen { get; } = new();
    public int ScreenLeft { get; } = 0;
    public int ScreenTop { get; } = 416;
    public float ScrollX { get; } = 0;
    public float ScrollY { get; } = 783.5f;
    public object VisualViewport { get; } = new();

    // I'm not so sure about these...
    public bool locationbar { get; } = true;
    public bool menubar { get; } = true;
    public bool personalbar { get; } = true;
    public bool scrollbars { get; } = true;
    public bool statusbar { get; } = true;
    public bool toolbar { get; } = true;
    //opener
    //parent: Window
    //top

    public async Task Alert(string message) { await Task.Delay(1); }
    public void Close() { }
    public void Confirm() { }
    public void Focus() { }
    public void GetComputedStyle() { }
    public void GetSelection() { }
    public void MatchMedia() { }
    public void MoveBy() { }
    public void MoveTo() { }
    public void Open() { }
    public void PostMessage() { }
    public void Print() { }

    public async Task<string> Prompt(string? message = null, string? defaultValue = null)
    {
        // return await transport.RpcRequest<string?, string?, string>("window.prompt", message, defaultValue);
        return "";
    }

    public void ResizeBy() { }
    public void ResizeTo() { }
    public void Scroll() { }
    public void ScrollBy() { }
    public void ScrollTo() { }
}