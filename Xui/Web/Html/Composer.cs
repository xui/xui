namespace Xui.Web;

internal class Composer
{
    internal Chunk[] chunks = new Chunk[1000];
    internal int cursor = 0;
    internal int depth = 0;
}