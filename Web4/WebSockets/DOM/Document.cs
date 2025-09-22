using Web4.Core.DOM;

namespace Web4.WebSockets;

partial class WebSocketTransport : IDocument
{
    IWindow IDocument.DefaultView => throw new NotImplementedException();

    string? IDocument.Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}