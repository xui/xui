using Web4;

namespace Web4.Transports;

public interface IWeb4Transport
{
    Window GetOrCreateWindow(WindowBuilder builder);
    ValueTask BeginMutations();
    ValueTask Mutate(IEnumerable<int> indexes, Snapshot before, Snapshot after);
    ValueTask EndMutations(CancellationToken cancel);
}