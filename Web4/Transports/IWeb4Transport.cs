using Web4;

namespace Web4.Transports;

public interface IWeb4Transport
{
    Window GetOrCreateWindow(WindowBuilder builder);
    ValueTask ApplyMutations(Keyhole[] oldBuffer, Keyhole[] newBuffer);
}