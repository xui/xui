using System.Buffers;

namespace Web4;

public class Snapshot : IDisposable
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int highWaterMark = 2048;
    
    public Keyhole[] Buffer { get; private set; }
    public Span<Keyhole> Root { get => Buffer.AsSpan()[..RootLength]; }
    
    public int BufferLength { get; internal set; } = 0;
    public int RootLength { get; internal set; } = 0;
    public Span<Keyhole> Keyholes { get => Buffer.AsSpan()[..BufferLength]; }

    public Snapshot()
    {
        Buffer = ArrayPool<Keyhole>.Shared.Rent(highWaterMark);
    }

    public void Dispose()
    {
        ArrayPool<Keyhole>.Shared.Return(Buffer);
    }
}