using Web4.Core.DOM;

namespace Web4.WebSockets;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
partial class WebSocketTransport : IDocument
{
    IWindow IDocument.DefaultView => throw new NotImplementedException();

    string? IDocument.Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}