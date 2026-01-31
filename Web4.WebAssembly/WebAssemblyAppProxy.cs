using System.Runtime.InteropServices.JavaScript;

namespace Web4.WebAssembly;

public partial class WebAssemblyAppProxy
{
    // [JSExport]
    internal static void Benchmark(int? threads) { }

    // [JSExport]
    internal static void Ping() { }
}