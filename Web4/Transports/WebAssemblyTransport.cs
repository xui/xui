
namespace Web4.Transports;

public class WebAssemblyTransport : IWeb4Transport, IDisposable
{
    public ValueTask BeginMutations()
    {
        throw new NotImplementedException();
    }

    public ValueTask EndMutations(CancellationToken cancel)
    {
        throw new NotImplementedException();
    }

    public Window GetOrCreateWindow(WindowBuilder builder)
    {
        throw new NotImplementedException();
    }

    public ValueTask Mutate(IEnumerable<int> indexes, Keyhole[] before, Keyhole[] after)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}