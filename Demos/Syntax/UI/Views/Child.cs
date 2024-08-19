partial class UI
{
    Html Child(MyViewModel vm, string? name = null) => $"""
        <p>{name ?? vm.Name ?? "(none)"}</p>
        """;
}