partial class UI
{
    Html Header(MyViewModel vm, string? nameOverride = null) => $"""
        <p>I am a header that might override name: {nameOverride ?? vm.Name ?? "(none)"}</p>
        """;
}