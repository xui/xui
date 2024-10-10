using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class IndexerComposer(int slotId) : BaseComposer
{
    public int SlotId { get; init; } = slotId;

    public Func<Event, Task>? EventHandler { get; set; } = null;

    private int cursor = 0;

    public override bool AppendFormatted(Action a)
    {
        if (cursor == SlotId)
        {
            EventHandler = @event => {
                a();
                return Task.CompletedTask;
            };
            
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(a);
    }

    public override bool AppendFormatted(Action<Event> a)
    {
        if (cursor == SlotId)
        {
            EventHandler = @event => {
                a(@event);
                return Task.CompletedTask;
            };
            
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(a);
    }

    public override bool AppendFormatted(Func<Task> f)
    {
        if (cursor == SlotId)
        {
            EventHandler = @event => {
                return f();
            };
            
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(f);
    }

    public override bool AppendFormatted(Func<Event, Task> f)
    {
        if (cursor == SlotId)
        {
            EventHandler = f;
            
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(f);
    }
}