using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class SearchComposer(int slotId) : BaseComposer
{
    public int SlotId { get; init; } = slotId;

    public Func<Event, Task>? EventHandler { get; set; } = null;

    public static Func<Event, Task>? GetSlot(int slotId, HtmlDelegate html)
    {
        var composer = new SearchComposer(slotId);
        composer.Compose($"{html()}");
        return composer.EventHandler;
    }

    public override bool AppendFormatted(Action eventHandler, string? expression = null)
    {
        if (Cursor == SlotId)
        {
            EventHandler = @event => {
                eventHandler();
                return Task.CompletedTask;
            };
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(eventHandler);
    }

    public override bool AppendFormatted(Action<Event> eventHandler, string? expression = null)
    {
        if (Cursor == SlotId)
        {
            EventHandler = @event => {
                eventHandler(@event);
                return Task.CompletedTask;
            };
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(eventHandler);
    }

    public override bool AppendFormatted(Func<Task> eventHandler, string? expression = null)
    {
        if (Cursor == SlotId)
        {
            EventHandler = @event => {
                return eventHandler();
            };
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(eventHandler);
    }

    public override bool AppendFormatted(Func<Event, Task> eventHandler, string? expression = null)
    {
        if (Cursor == SlotId)
        {
            EventHandler = eventHandler;
            
            base.Clear();
            // Save time. Short circuits any following appends.
            return false;
        }

        return base.AppendFormatted(eventHandler);
    }

    // We have an unfortunate edge case to handle here.  
    // The notation used for some attributes:
    //   $"<input type="text" { maxlength => c } />"
    // technically also matches the signature used for events:
    //   $"<button { onclick => c++ }>click me</button>"
    // Fortunately there's a simple workaround.  Since attributes
    // only use the input param for its name, never its value 
    // we can just key off its name and send it down a different path
    // as if it were an event handler.
    //
    // AppendFormatted(@event => f(@event)) ...returns T
    // AppendFormatted(@event => { f(@event); }) ...returns void
    public override bool AppendFormatted<T>(Func<Event, T> f, string? format = null, string? expression = null) => AppendFormatted(e => { f(e); });
    public override bool AppendFormatted(Func<Event, bool> f, string? expression = null) => AppendFormatted(e => { f(e); });
}