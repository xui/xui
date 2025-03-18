using System.Runtime.InteropServices.JavaScript;

public partial class Sample
{
    // Make the method accessible from JS
    [JSExport]
    internal static int Add(int a, int b)
    {
        return a + b;
    }
}