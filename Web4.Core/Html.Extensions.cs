namespace Web4;

public static class HtmlExtensions
{
    public static Html.Enumerable<T> Select<T>(this IEnumerable<T> source, Func<T, Html> selector)
        => new(source, selector);
}