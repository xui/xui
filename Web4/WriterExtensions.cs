using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Web4.Composers;

namespace Web4;

public static class WriterExtensions
{
    public static void WriteRaw(
        this IBufferWriter<byte> writer,
        [InterpolatedStringHandlerArgument("writer")] ref RawText text)
    {
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        [InterpolatedStringHandlerArgument("writer", "composer")] ref Html html,
        CancellationToken cancel = default)
    {
        html.Dispose();
        return writer.FlushAsync(cancel);
    }

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        Func<Html> template,
        HttpContext http,
        bool includeServerTiming = false,
        CancellationToken cancel = default)
            => includeServerTiming
                ? writer.WriteWithServerTimingAsync(composer, template, http, cancel)
                : writer.WriteAsync(composer, $"{template()}", cancel);

    public static ValueTask<FlushResult> WriteAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        Func<Html> template,
        HttpContext http)
    {
        // TODO: Move to config.  Server-timing in RELEASE mode should be possible.
#if DEBUG
        return writer.WriteWithServerTimingAsync(composer, template, http, http.RequestAborted);
#else
        return writer.WriteAsync(composer, $"{template()}", http.RequestAborted);        
#endif
    }

    public static ValueTask<FlushResult> WriteWithServerTimingAsync(
        this PipeWriter writer,
        StreamingComposer composer,
        Func<Html> template,
        HttpContext http,
        CancellationToken cancel = default)
    {
        long gc1 = GC.GetAllocatedBytesForCurrentThread();
        long stopwatch = Stopwatch.GetTimestamp();

        writer.Write(composer, $"{template()}");

        var elapsed = Stopwatch.GetElapsedTime(stopwatch);
        long gc2 = GC.GetAllocatedBytesForCurrentThread();

        // Ironically this allocates.  But it occurs after measurement and only in DEBUG.
        http.Response.Headers["Server-Timing"] = $"""
            allocations;desc="Allocations: {gc2 - gc1}b", render;desc="Web4.Render";dur={elapsed.TotalNanoseconds / 1_000_000d}
            """;

        return writer.FlushAsync(cancel);
    }
}