global using Web4;
using System.Drawing;
using System.IO.Pipelines;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Web4.Composers;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

[ShortRunJob]
[MemoryDiagnoser]
[OperationsPerSecond]
public class Tests
{
    readonly Pipe pipe = new();
    readonly NoOpComposer noOpComposer = new();
    readonly NoOpWriter noOpWriter = new();
    readonly HtmlComposer htmlComposer = new(null);
    readonly XtmlComposer xtmlComposer = new(null, new(null, null));
    readonly SnapshotComposer snapshotComposer = new();
    readonly FindKeyholeComposer findKeyholeComposer = new();
    readonly string name = "Rylan";
    readonly int c = 3;
    readonly int cState = 3;// State<int> cState = 3.AsState();

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
        noOpComposer.Compose($"""
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
        noOpComposer.Compose($"""
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




    [Benchmark]
    public void InlineAsInterpolated()
    {
        noOpComposer.Compose($"""
            <div>
                {$"<p>Hello {name}<p>"}
            </div>
            """);
    }

    [Benchmark]
    public void InlineAsMethod()
    {
        noOpComposer.Compose($"""
            <div>
                {GetInline()}
            </div>
            """);
    }

    Html GetInline() => $"<p>Hello {name}</p>";




    // readonly static BaseComposer baseComposer = new();
    record Point(double X, double Y);
    private static Point[] tiles;

    [Benchmark]
    public void SpiralToDevNull()
    {
        // noOpWriter.Write(noOpComposer, $$"""
        // baseComposer.Compose($$"""
        noOpComposer.Compose($$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SpiralToUtf8Buffer()
    {
        noOpWriter.Write(htmlComposer, $$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SpiralToUtf8Xtml()
    {
        noOpWriter.Write(xtmlComposer, $$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void SpiralSearch()
    {
        Func<Html> template = () => $$"""
            <!DOCTYPE html>
            <html>
                <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                body {
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    background-color: #f0f0f0;
                    margin: 0;
                }
                #wrapper {
                    width: 960px;
                    height: 720px;
                    position: relative;
                    background-color: white;
                }
                .tile {
                    position: absolute;
                    width: 10px;
                    height: 10px;
                    background-color: #333;
                }
                </style>
                </head>
                <body>
                <div id="root">
                    {{ tiles.Select(t => Tile(t.X, t.Y)) }}
                </div>
                </body>
            </html>
            """;
        template.FindEventListener("");
    }

    static Html Tile(double x, double y) => $"""
        <div 
            class="tile"
            style="left: {x:0.00}px; top: {y:0.00}px;">
        </div>
        """;





    static readonly string web4Assets = ".";
    static readonly string web4Head = "";
    static EntryRecord[] entries;
    
    [Benchmark]
    public void GuidTableToDevNull()
    {
        noOpComposer.Compose($"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="{web4Assets}/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    {web4Head}
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """);
    }
    
    [Benchmark]
    public void GuidTableToUtf8Buffer()
    {
        noOpWriter.Write(htmlComposer, $"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="{web4Assets}/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    {web4Head}
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void GuidTableToUtf8Xtml()
    {
        noOpWriter.Write(xtmlComposer, $"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="{web4Assets}/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    {web4Head}
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """);
    }

    [Benchmark]
    public void GuidTableSearch()
    {
        Func<Html> template = () => $"""
            <!doctype html>
            <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <link rel="icon" href="{web4Assets}/favicon.png" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    {web4Head}
                </head>
                <body data-sveltekit-preload-data="hover">
                    <div style="display: contents">{GuidTableBody()}</div>
                </body>
            </html>
            """;
        template.FindEventListener("");
    }

    static Html GuidTableBody() => $"""
        <main>
          <table>
            {from entry in entries
             select Entry(entry)}
          </table>
        </main>
    """;

    static Html Entry(EntryRecord entry) => $"""
    <tr>
      <td>{entry.ID}</td>
      <td>{entry.Name}</td>
    </tr>
    """;

    record class EntryRecord(string ID, string Name);



    static Tests()
    {
        var wrapperWidth = 960;
        var wrapperHeight = 720;
        var cellSize = 10;
        var centerX = wrapperWidth / 2d;
        var centerY = wrapperHeight / 2d;

        var angle = 0d;
        var radius = 0d;

        var tiles = new List<Point>();
        var step = cellSize;

        while (radius < Math.Min(wrapperWidth, wrapperHeight) / 2d)
        {
            var x = centerX + Math.Cos(angle) * radius;
            var y = centerY + Math.Sin(angle) * radius;

            if (x >= 0 && x <= wrapperWidth - cellSize && y >= 0 && y <= wrapperHeight - cellSize)
            {
                tiles.Add(new(x, y));
            }

            angle += 0.2;
            radius += step * 0.015;
        }

        Tests.tiles = tiles.ToArray();




        Tests.entries = [.. Enumerable
            .Range(0, 1000)
            .Select(i => new EntryRecord(
                ID: Guid.NewGuid().ToString(),
                Name: Guid.NewGuid().ToString()
            ))
        ];
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

