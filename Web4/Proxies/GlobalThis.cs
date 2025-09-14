namespace Web4.Proxies;

public readonly ref struct GlobalThis(IWeb4Transport transport)
{
    public Web4App App { get; init; }
    public WindowProxy Window { get; init; }
    public DocumentProxy Document { get; init; }
    public ConsoleProxy Console { get; init; }
}