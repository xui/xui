using Web4.Core.DOM;

namespace Web4.WebSockets;

partial class WebSocketTransport : IWindow
{

    IConsole IWindow.Console => throw new NotImplementedException();

    IDocument IWindow.Document => throw new NotImplementedException();

    IWindow IWindow.Window => throw new NotImplementedException();

    void IWindow.Alert(string message)
    {
        throw new NotImplementedException();
    }

    void IWindow.Focus()
    {
        throw new NotImplementedException();
    }

    Task<string> IWindow.Prompt(string? message, string? defaultValue)
    {
        throw new NotImplementedException();
    }
}