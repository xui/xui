namespace Web4.HttpX;

public interface IViewModel
{
    static abstract IViewModel New();
    Action? OnChanged { get; set; }
    IDisposable Batch();
}