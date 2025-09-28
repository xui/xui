using Web4.Core.DOM;

namespace Web4;

public interface IWeb4Transport
{
    Web4App App { get; }
    // Web4App GetOrCreateApp(WindowBuilder builder);
    ValueTask ApplyMutations(Keyhole[] oldBuffer, Keyhole[] newBuffer);
}