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

    public static ValueTask<FlushResult> WriteAsync<T>(
        this PipeWriter writer,
        T composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html,
        CancellationToken cancel = default)
            where T : struct, IStreamingComposer
    {
        // When instantiating an Html object, the compiler generates 
        // lowered code (i.e. AppendLiteral and AppendFormatted) which
        // is where the bytes get written to the PipeWriter.

        return writer.FlushAsync(cancel);
    }

    public static ValueTask<FlushResult> WriteAsync<T>(
        this PipeWriter writer,
        T composer,
        Func<Html> html,
        HttpContext http,
        bool includeServerTiming = false,
        CancellationToken cancel = default)
            where T : struct, IStreamingComposer
            => includeServerTiming
                ? writer.WriteWithServerTimingAsync(composer, html, http, cancel)
                : writer.WriteAsync(composer, $"{html()}", cancel);

    public static ValueTask<FlushResult> WriteAsync<T>(
        this PipeWriter writer,
        T composer,
        Func<Html> html,
        HttpContext http)
            where T : struct, IStreamingComposer
    {
        // TODO: Move to config.  Server-timing in RELEASE mode should be possible.
#if DEBUG
        return writer.WriteWithServerTimingAsync(composer, html, http, http.RequestAborted);
#else
        return writer.WriteAsync(composer, $"{html()}", http.RequestAborted);        
#endif
    }

    public static ValueTask<FlushResult> WriteWithServerTimingAsync<T>(
        this PipeWriter writer,
        T composer,
        Func<Html> html,
        HttpContext http,
        CancellationToken cancel = default)
            where T : struct, IStreamingComposer
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

    public static void Compose<T>(
        this T composer,
        [InterpolatedStringHandlerArgument("composer")] Html html)
            where T : struct, IComposer
    {
        // TODO: This can be moved to the BaseComposer.cs file, use empty string as the arg.
        // This strange gymnastics is required because InterpolatedStringHandlerArgument
        // must have at least one arg before it in order for the compiler to pick it up right.  
        // Extension methods help create the illusion of a simple composer.Compose($"...").
    }
}