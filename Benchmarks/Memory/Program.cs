global using Web4;
using System.Drawing;
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
    HttpXComposer httpXComposer = new(null, new(null, null));
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
    public async Task HttpXComposer()
    {
        pipe.Writer.Write(httpXComposer, $"""
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
    public async Task HttpXComposerWithState()
    {
        pipe.Writer.Write(httpXComposer, $"""
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

    int i = 1;
    long l = 1;
    float f = 3.14f;
    double d = 3.14;
    decimal m = 3.14m;
    DateTime dt = DateTime.Now;
    DateOnly d0 = DateOnly.MaxValue;
    TimeSpan ts = TimeSpan.MaxValue;
    TimeOnly t0 = TimeOnly.MaxValue;
    bool b = true;
    Color color = Color.Red;
    string str = "str";
    Uri uri = new Uri("https://web4.dev");


    [Benchmark]
    public void DiffComposerInt()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {i}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerLong()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {l}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerFloat()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {f}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerDouble()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {d}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerDecimal()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {m}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerDateTime()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {dt}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerDateOnly()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {d0}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerTimeSpan()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {ts}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerTimeOnly()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {t0}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerBool()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {b}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerColor()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {color}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerString()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {str}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerUri()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {uri}
                    </button>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void DiffComposerFormatString()
    {
        diffComposer.Compose($"""
            <html>
                <body>
                    <button>
                        Clicks: {c:c}
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

