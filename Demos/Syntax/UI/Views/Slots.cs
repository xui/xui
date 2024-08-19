partial class UI
{
    // Problem here, Func<T> isn't a value type.
    // Find another way to pass children without them jumping the gun onto the buffer.

    Html HasContent(Func<Html> content) => $"""
        <h2 style="color: red">
            {content()}
        </h2>
    """;

    Html Slots(
        Func<Html> title,
        Func<Html> caption,
        Func<Html> content
    ) => $"""
        <h2>{title()}</h2>
        <code>{caption()}</code>
        <p style="color: red">
            {content()}
        </p>
    """;

}