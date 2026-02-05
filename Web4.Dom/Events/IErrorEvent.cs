using static Web4.Dom.Events.Aliases.Subsets;

namespace Web4.Dom.Events;

public interface IErrorEvent
    : IEvent, Error
{
}

public record struct DOMException(string Name = "", string Message = "")
{
    public static readonly DOMException Empty = new();
}