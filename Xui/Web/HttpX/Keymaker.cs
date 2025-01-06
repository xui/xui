
namespace Xui.Web.HttpX;

internal struct Keymaker
{
    private static readonly char[] ALPHANUMERICS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public static string GetKey(string parentKey, int cursor)
    {
        return parentKey + ALPHANUMERICS[cursor];
    }
}