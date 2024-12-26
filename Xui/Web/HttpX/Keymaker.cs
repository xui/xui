namespace Xui.Web.HttpX;

public struct Keymaker
{
    private static readonly char[] ALPHANUMERICS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static int depth = 0;
    private static int cursor = 0;

    private static char[] key = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA".ToCharArray();

    public static void MoveDown(int depth)
    {
        cursor = 0;
        Keymaker.depth += depth;
    }

    public static void MoveUp(int depth)
    {
        cursor = 0;
        Keymaker.depth -= depth;
    }

    public static string GetNext()
    {
        key[depth-1] = ALPHANUMERICS[cursor++];
        return new string(key[..depth]);
    }
}