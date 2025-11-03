using System.Buffers;

namespace Web4.WebSockets;

public class PooledSequenceBufferWriter<T> : IBufferWriter<T>
{
    private const int DEFAULT_BUFFER_SIZE = 16384;
    private T[]? currentBuffer;
    private int currentIndex;
    private SequenceSegment<T>? segmentStart;
    private SequenceSegment<T>? segmentEnd;

    public ReadOnlySequence<T> Sequence
    {
        get
        {
            var sequence = this switch
            {
                // Never written to
                _ when currentBuffer is null => ReadOnlySequence<T>.Empty,

                // Single-segment
                _ when segmentEnd is null => new ReadOnlySequence<T>(currentBuffer, 0, currentIndex),

                // Multi-segment
                _ => new ReadOnlySequence<T>(segmentStart!, 0, segmentEnd.Append(currentBuffer!, 0..currentIndex), currentIndex)
            };

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
        var unusedCapacity = (currentBuffer?.Length ?? 0) - currentIndex;
        var bufferLength = Math.Max(sizeHint, DEFAULT_BUFFER_SIZE);

        // Return early if we already have enough capacity.
        if (unusedCapacity > sizeHint)
            return;

        // If this is already multi-segment, append the current buffer into 
        // the linked list and rent a fresh one.
        if (segmentEnd is not null)
        {
            segmentEnd = segmentEnd.Append(currentBuffer!, 0..currentIndex);
            currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            currentIndex = 0;
            return;
        }

        // Single-segment is out of space.  Convert this to multi-segment.
        if (currentBuffer is not null)
        {
            // SequenceSegment allocates memory 
            // and evidently there's no way around it short of a grow-copy-buffer approach.
            segmentStart = new SequenceSegment<T>(currentBuffer, 0..currentIndex);
            segmentEnd = segmentStart;
            currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            currentIndex = 0;
            return;
        }

        // Must be the first write.  Rent a fresh buffer.
        if (currentBuffer is null)
        {
            currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            currentIndex = 0;
            return;
        }
    }
}