using Web4.Events;
using Web4.Events.Subsets;

namespace Web4;

public static class EventExtensions
{
    public static Context GetContext(this IEvent e)
    {
        return new();
    }

    public static Context GetContext(this ISubset e)
    {
        return new();
    }
}