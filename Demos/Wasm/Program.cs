using System.Runtime.InteropServices.JavaScript;

// Create a "Main" method. This is required by the tooling.
return;

public partial class Sample
{
    // Make the method accessible from JS
    [JSExport]
    internal static int Add(int a, int b)
    {
        return a + b;
    }
}
