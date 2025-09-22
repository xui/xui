namespace Web4.Core.DOM;

public interface IWindow
{
    IConsole Console { get; }
    IDocument Document { get; }
    IWindow Window { get; }
}
