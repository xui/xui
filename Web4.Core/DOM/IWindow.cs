namespace Web4.Core.DOM;

public interface IWindow
{
    IConsole Console { get; }
    IDocument Document { get; }
    IWindow Window { get; }

    void Focus();
    void Alert(string message);
    Task<string> Prompt(string? message = null, string? defaultValue = null);
}
