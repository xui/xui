namespace Web4.WebSockets;

internal class EventListenerSynchronizationContext(WebSocketTransport transport)
    : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object? state)
    {
        base.Post(s =>
        {
            SetSynchronizationContext(this); // because AspNetWebSocket uses ConfigureAwait(false)
            d(s);
            transport.Flush();
        }, state);
    }
}