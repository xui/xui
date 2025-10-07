using System.Buffers;

namespace Web4.WebSockets;

public class PooledSequenceBufferWriter<T> : IBufferWriter<T>
{
    private const int DEFAULT_BUFFER_SIZE = 4096;
    private T[]? currentBuffer;
    private int currentIndex;
    private SequenceSegment<T>? segmentStart;
    private SequenceSegment<T>? segmentEnd;

    public ReadOnlySequence<T> Sequence
    {
        get
        {
            var sequence = segmentStart is not null && segmentEnd is not null
                ? new ReadOnlySequence<T>(segmentStart, 0, segmentEnd, currentIndex)
                : currentBuffer is not null
                    ? new ReadOnlySequence<T>(currentBuffer, 0, currentIndex)
                    : ReadOnlySequence<T>.Empty;

            Reset();

            return sequence;
        }
    }
    public int WrittenCount { get; private set; }

    private void Reset()
    {
        WrittenCount = 0;
        currentBuffer = null;
        currentIndex = 0;
        segmentStart = null;
        segmentEnd = null;
    }

    public void Advance(int count)
    {
        currentIndex += count;
        WrittenCount += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return currentBuffer.AsMemory(currentIndex);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return currentBuffer.AsSpan(currentIndex);
    }

    private void GrowIfNeeded(int sizeHint)
    {
        var bufferLength = Math.Max(sizeHint, DEFAULT_BUFFER_SIZE);
        if (currentBuffer is null)
        {
            currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            currentIndex = 0;
        }
        else
        {
            var freeCapacity = currentBuffer.Length - currentIndex;
            if (freeCapacity < sizeHint)
            {
                if (segmentStart is null || segmentEnd is null)
                {
                    // TODO: new SequenceSegment() allocates memory
                    segmentStart = new SequenceSegment<T>(currentBuffer, 0..currentIndex);
                    segmentEnd = segmentStart;
                }
                currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
                segmentEnd = segmentEnd.Append(currentBuffer, 0..currentIndex);
                currentIndex = 0;
            }
        }
    }
}