using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Web4.Composers;

namespace Web4;

public static class WriterExtensions
{
    public static void Inject(
        this IBufferWriter<byte> writer, 
        [InterpolatedStringHandlerArgument("writer")] ref RawText text)
    {
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer, 
        [InterpolatedStringHandlerArgument("writer")] ref Html html,
        CancellationToken cancellationToken = default)
    {
        // When instantiating an Html object, the compiler generates 
        // lowered code (i.e. AppendLiteral and AppendFormatted) which
        // is where the bytes get written to the PipeWriter.

        return writer.FlushAsync(cancellationToken);
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer, 
        StreamingComposer composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html,
        CancellationToken cancellationToken = default)
    {
        // When instantiating an Html object, the compiler generates 
        // lowered code (i.e. AppendLiteral and AppendFormatted) which
        // is where the bytes get written to the PipeWriter.

        return writer.FlushAsync(cancellationToken);
    }

    public static void Compose(
        this BaseComposer composer, 
        [InterpolatedStringHandlerArgument("composer")] Html html)
    {
        // This strange gymnastics is required because InterpolatedStringHandlerArgument
        // must have at least one arg before it in order for the compiler to pick it up right.  
        // Extension methods help create the illusion of a simple composer.Compose($"...").
    }
}