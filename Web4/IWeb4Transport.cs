namespace Web4;

public interface IWeb4Transport
{
    Window GetOrCreateWindow(WindowBuilder builder);
    ValueTask ApplyMutations(Keyhole[] oldBuffer, Keyhole[] newBuffer);
}