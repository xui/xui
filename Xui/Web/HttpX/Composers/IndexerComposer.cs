using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class IndexerComposer(int slotId) : BaseComposer
{
    public int SlotId { get; init; } = slotId;

    public Func<Event, Task>? EventHandler { get; set; } = null;

    public static Func<Event, Task>? GetSlot(int slotId, HtmlDelegate html)
    {
        var composer = new IndexerComposer(slotId);
        composer.Compose($"{html()}");
        return composer.EventHandler;
    }

    public override bool AppendFormatted(Action a)
    {
        if (Cursor == SlotId)
        {
            EventHandler = @event => {
                a();
                return Task.CompletedTask;
            };
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(a);
    }

    public override bool AppendFormatted(Action<Event> a)
    {
        if (Cursor == SlotId)
        {
            EventHandler = @event => {
                a(@event);
                return Task.CompletedTask;
            };
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(a);
    }

    public override bool AppendFormatted(Func<Task> f)
    {
        if (Cursor == SlotId)
        {
            EventHandler = @event => {
                return f();
            };
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(f);
    }

    public override bool AppendFormatted(Func<Event, Task> f)
    {
        if (Cursor == SlotId)
        {
            EventHandler = f;
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(f);
    }
}