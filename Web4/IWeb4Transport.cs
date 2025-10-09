using Web4.Core.DOM;

namespace Web4;

public interface IWeb4Transport : IWindow, IDocument, IConsole
{
    Web4App App { get; }
    void Diff(Keyhole[] oldBuffer, Keyhole[] newBuffer);
}