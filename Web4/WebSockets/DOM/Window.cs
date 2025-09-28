using Web4.Core.DOM;

namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
partial class WebSocketTransport : IWindow
{

    IConsole IWindow.Console => this;

    IDocument IWindow.Document => this;

    IWindow IWindow.Window => this;

    void IWindow.Alert(string message)
    {
        BatchWriter.WriteNotification("window.alert", message);
    }

    void IWindow.Focus()
    {
        BatchWriter.WriteNotification("window.focus");
    }

    Task<string> IWindow.Prompt(string? message, string? defaultValue)
    {
        throw new NotImplementedException();
    }
}