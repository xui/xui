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
    HtmlComposer htmlComposer = new(null);
    XtmlComposer xtmlComposer = new(null, new(null));
    SnapshotComposer snapshotComposer = new();
    string name = "Rylan";
    int c = 3;
    State<int> cState = 3.AsState();

    [Benchmark]
    public void Baseline()
    {
        NoOpString noOpString = $"""
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
    public async Task HtmlComposer()
    {
        pipe.Writer.Write(htmlComposer, $"""
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
    public async Task HtmlComposerWithState()
    {
        pipe.Writer.Write(htmlComposer, $"""
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
    public async Task XtmlComposer()
    {
        pipe.Writer.Write(xtmlComposer, $"""
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
    public async Task XtmlComposerWithState()
    {
        pipe.Writer.Write(xtmlComposer, $"""
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
    public void SnapshotComposerInt()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerLong()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerFloat()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerDouble()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerDecimal()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerDateTime()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerDateOnly()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerTimeSpan()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerTimeOnly()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerBool()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerColor()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerString()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerUri()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerFormatString()
    {
        snapshotComposer.Compose($"""
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
    public void SnapshotComposerWithState()
    {
        snapshotComposer.Compose($"""
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
//         bool noop = false;
//         if (args.Length > 1)
//         {
//             threads = int.Parse(args[1]);
//             noop = args[^1] == "noop";
//         }
//         Threads.Test(threads, noop);
//         break;
//     default:
//         throw new NotSupportedException("");
// }

