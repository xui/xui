namespace Web4.Proxies;

public struct DocumentProxy(IWeb4Transport transport)
{
    public string? Title { get; set; }
    
    public readonly WindowProxy DefaultView { get => new(transport); }
}

