global using Web4;
using System.IO.Pipelines;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Web4.Composers;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[ShortRunJob]
[MemoryDiagnoser]
public class Tests
{
    Pipe pipe = new();
    NoOpComposer noOpComposer = new(null);
    DefaultComposer defaultComposer = new(null);
    DiffComposer diffComposer = new();
    string name = "Rylan";
    int c = 3;
    State<int> cState = 3.AsState();

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
    public async Task NoOpComposer()
    {
        pipe.Writer.Write(noOpComposer, $"""
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

    [Benchmark]
    public async Task NoOpComposerWithState()
    {
        pipe.Writer.Write(noOpComposer, $"""
            <html>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {cState}
                    </button>
                </body>
            </html>
            """);

        await pipe.Writer.FlushAsync();
        if (pipe.Reader.TryRead(out ReadResult result))
            pipe.Reader.AdvanceTo(result.Buffer.End);
    }

    [Benchmark]
    public async Task DefaultComposer()
    {
        pipe.Writer.Write(defaultComposer, $"""
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

    [Benchmark]
    public async Task DefaultComposerWithState()
    {
        pipe.Writer.Write(defaultComposer, $"""
            <html>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {cState}
                    </button>
                </body>
            </html>
            """);

        await pipe.Writer.FlushAsync();
        if (pipe.Reader.TryRead(out ReadResult result))
            pipe.Reader.AdvanceTo(result.Buffer.End);
    }

    [Benchmark]
    public void DiffComposer()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {c}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerWithState()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    Hello {name}
                    <button>
                        Clicks: {cState}
                    </button>
                </body>
            </html>
            """);
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

