namespace Web4;

public static class HtmlExtensions
{
    public static Html.Enumerable<T> Select<T>(this IEnumerable<T> source, Func<T, Html> selector)
        => new(source, selector);
}

public ref partial struct Html
{
    public struct Enumerable<T>(IEnumerable<T> source, Func<T, Html> selector)
    {
        public readonly int Count => source.Count();
        public readonly Enumerator<T> GetEnumerator() => new(source, selector);
    }

    public struct Enumerator<T>(IEnumerable<T> source, Func<T, Html> selector)
    {
        int index = -1;
        readonly IEnumerator<T>? enumerator = source is not IList<T> ? source.GetEnumerator() : null;

        public readonly Html Current => source switch
        {
            IList<T> list => selector(list[index]),
            _ => selector(enumerator!.Current),
        };

        public bool MoveNext() => source switch
        {
            IList<T> list => ++index < list.Count,
            _ => enumerator!.MoveNext(),
        };
    }
}