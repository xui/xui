using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;
using Xui.Web.HttpX.Composers;

namespace Xui.Web.HttpX;

public static class Extensions
{
    public static ValueTask<FlushResult> WriteAsync(this Func<Html> html, PipeWriter pipeWriter, bool sentinels = false)
    {
        if (sentinels)
        {
            var composer = new HttpXComposer(pipeWriter);
            return pipeWriter.WriteAsync(composer, $"{html()}");
        }
        else
        {
            var composer = new DefaultComposer(pipeWriter);
            return pipeWriter.WriteAsync(composer, $"{html()}");
        }
    }
    
    public static Func<Event?, Task>? GetKeyhole(this Func<Html> html, string? key)
    {
        if (key is null)
            return null;

        var composer = new FindKeyholeComposer(key);
        composer.Compose($"{html()}");
        return composer.EventHandler;
    }

    private static bool warmedUp = false;
    public static async Task DebugSnapshot(this Func<Html> html, PipeWriter writer)
    {
        try
        {
            var composer = new DiffComposer();
            // // Warmup...
            // var warmup = Stopwatch.StartNew();
            // long c = 0;
            // while (warmup.ElapsedMilliseconds < 1000)
            // {
            //     c++;
            // }
            // Console.WriteLine(c);
            // if (!warmedUp)
            // {
                // for (int i = 0; i < 250_000; i++)
                    // composer.Compose($"{html()}");
            //     warmedUp = true;
            // }

            // long gc1 = GC.GetAllocatedBytesForCurrentThread();
            // var sw1 = Stopwatch.GetTimestamp();
            // for (int i = 0; i < 250_000; i++)
                composer.Compose($"{html()}");
            // var elapsed = Stopwatch.GetElapsedTime(sw1);
            // long gc2 = GC.GetAllocatedBytesForCurrentThread();
            // Console.WriteLine($"elapsed: {elapsed.TotalNanoseconds} ns, allocations: {(gc2 - gc1):n0} bytes");

            var output = Debug.GetOutput(composer);
            writer.Inject($"{output.ToString()}");
            await writer.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}