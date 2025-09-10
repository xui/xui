namespace Web4;

public interface IWeb4Transport
{
    Web4App GetOrCreateApp(WindowBuilder builder);
    ValueTask ApplyMutations(Keyhole[] oldBuffer, Keyhole[] newBuffer);
}