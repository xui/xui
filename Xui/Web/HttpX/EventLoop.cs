using System.Collections.Concurrent;

namespace Xui.Web.HttpX;

static class EventLoop
{
    private static readonly ConcurrentQueue<Action> queue = [];

    public static void Enqueue(Func<Event, Task> eventHandler, Event domEvent)
    {
        // TODO: Do the work.
        eventHandler(domEvent);
    }
}