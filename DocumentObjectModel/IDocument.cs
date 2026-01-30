namespace Web4.Core.DOM;

public interface IDocument
{
    IWindow DefaultView { get; }
    string? Title { get; set; }
}
