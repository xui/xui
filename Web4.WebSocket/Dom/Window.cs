using Web4.Dom;

namespace Web4.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
// TODO: Ignore that.  Make it an `internal struct` like LazyEvent?
partial class WebSocketTransport : IWindow
{

    IConsole IWindow.Console => this;

    IDocument IWindow.Document => this;

    IWindow IWindow.Window => this;

    void IWindow.Alert(string message)
    {
        Output.WriteNotification("window.alert", message);
    }

    void IWindow.Focus()
    {
        Output.WriteNotification("window.focus");
    }

    Task<string> IWindow.Prompt(string? message, string? defaultValue)
    {
        throw new NotImplementedException();
    }
}