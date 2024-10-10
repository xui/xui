global using Xui.Web;
global using Xui.Web.HttpX;
using System.IO.Pipelines;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Xui.Web.Composers;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[ShortRunJob]
[MemoryDiagnoser]
public class Tests
{
    Pipe pipe = new();
    DefaultComposer composer = new(null);
    string name = "Rylan";
    int c = 3;

    [Benchmark]
    public void Baseline()
    {
        Placebo placebo = $"""
        <html>
            <body>
                Hello {name}
                <button>
                    Clicks: {c}
                </button>
            </body>
        </html>
        """;
    }

    [Benchmark]
    public async Task Small()
    {
        var eventHandlers = pipe.Writer.Write(composer, $"""
        <html>
            <body>
                Hello {name}
                <button>
                    Clicks: {c}
                </button>
            </body>
        </html>
        """);

        await pipe.Writer.FlushAsync();
        if (pipe.Reader.TryRead(out ReadResult result))
            pipe.Reader.AdvanceTo(result.Buffer.End);
    }
}








// if (args.Length == 0)
// {
//     Console.WriteLine("Which test?\n  benchmarkdotnet\n  threads n");
//     return;
// }

// switch (args[0])
// {
//     case "benchmarkdotnet":
//         BenchmarkRunner.Run<UI>();
//         break;
//     case "threads":
//         int threads = 1;
//         bool placebo = false;
//         if (args.Length > 1)
//         {
//             threads = int.Parse(args[1]);
//             placebo = args[^1] == "placebo";
//         }
//         Threads.Test(threads, placebo);
//         break;
//     default:
//         throw new NotSupportedException("");
// }

