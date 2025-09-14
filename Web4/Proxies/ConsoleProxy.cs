namespace Web4.Proxies;

public struct ConsoleProxy(IWeb4Transport transport)
{
    public void Log(string message)
    {
        // transport.RpcNotification("console.log", message);
    }
}