
namespace Xui.Web.HttpX;

internal struct Keymaker
{
    private static readonly char[] ALPHANUMERICS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    private static string parentKey = string.Empty;
    private static int cursor = 0;

    public static void Reset(string parentKey, int cursor)
    {
        Keymaker.parentKey = parentKey;
        Keymaker.cursor = cursor;
    }

    public static string GetNext()
    {
        return parentKey + ALPHANUMERICS[cursor++];
    }

    public static string GetKey(ref Html parent)
    {
        return parent.Key + ALPHANUMERICS[parent.Cursor / 2];
    }
}