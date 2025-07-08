using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
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
        CancellationToken cancel = default)
    {
        // When instantiating an Html object, the compiler generates 
        // lowered code (i.e. AppendLiteral and AppendFormatted) which
        // is where the bytes get written to the PipeWriter.

        return writer.FlushAsync(cancel);
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html,
        CancellationToken cancel = default)
    {
        // When instantiating an Html object, the compiler generates 
        // lowered code (i.e. AppendLiteral and AppendFormatted) which
        // is where the bytes get written to the PipeWriter.

        return writer.FlushAsync(cancel);
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        Func<Html> html,
        HttpContext http,
        bool includeServerTiming = false,
        CancellationToken cancel = default)
            => includeServerTiming
                ? writer.WriteWithServerTimingAsync(composer, html, http, cancel)
                : writer.WriteAsync(composer, $"{html()}", cancel);

    public static ValueTask<FlushResult> WriteWithServerTimingAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        Func<Html> html,
        HttpContext http,
        CancellationToken cancel = default)
    {
        long gc1 = GC.GetAllocatedBytesForCurrentThread();
        long stopwatch = Stopwatch.GetTimestamp();

        writer.Write(composer, $"{html()}");

        var elapsed = Stopwatch.GetElapsedTime(stopwatch);
        long gc2 = GC.GetAllocatedBytesForCurrentThread();

        // Ironically this allocates.  But it occurs after measurement and only in DEBUG.
        http.Response.Headers["Server-Timing"] = $"""
            allocations;desc="Allocations: {gc2 - gc1}b", render;desc="Web4.Render";dur={elapsed.TotalNanoseconds / 1_000_000d}
            """;

        return writer.FlushAsync(cancel);
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