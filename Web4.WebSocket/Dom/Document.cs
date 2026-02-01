using Web4.Dom;

namespace Web4.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on WebSocketTransport.
// TODO: Ignore that.  Make it an `internal struct` like LazyEvent?
internal partial class WebSocketTransport : IDocument
{
    IWindow IDocument.DefaultView => throw new NotImplementedException();

    string? IDocument.Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}