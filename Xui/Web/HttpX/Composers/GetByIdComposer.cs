using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class GetByIdComposer(int slotId) : BaseComposer
{
    public int SlotId { get; init; } = slotId;

    public Func<Event, Task>? EventHandler { get; set; } = null;

    public static Func<Event, Task>? GetSlot(int slotId, HtmlDelegate html)
    {
        var composer = new GetByIdComposer(slotId);
        composer.Compose($"{html()}");
        return composer.EventHandler;
    }

    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Action eventHandler) => ToCommonSignatureIfMatch(eventHandler);
    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Action<Event> eventHandler) => ToCommonSignatureIfMatch(eventHandler);
    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Func<Task> eventHandler) => ToCommonSignatureIfMatch(eventHandler);
    public override bool AppendEventHandler(ReadOnlySpan<char> argName, Func<Event, Task> eventHandler) => ToCommonSignatureIfMatch(eventHandler);

    private bool ToCommonSignatureIfMatch<T>(T eventHandler)
    {
        if (Cursor != SlotId)
        {
            return base.CompleteDynamic(1);
        }

        EventHandler = eventHandler switch
        {
            // Example:
            //   void OnClick()
            //   {
            //       Console.WriteLine($"Button was clicked");
            //   }
            _ when eventHandler is Action action => Map(action),

            // Example:
            //   void OnClick(Event e)
            //   {
            //       Console.WriteLine($"Button {e.currentTarget.id} was clicked");
            //   }
            _ when eventHandler is Action<Event> action => Map(action),

            // Example:
            //   async Task OnClick()
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button was clicked");
            //   }
            _ when eventHandler is Func<Task> func => Map(func),

            // Example:
            //   async Task OnClick(Event e)
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button {e.currentTarget.id} was clicked");
            //   }
            _ when eventHandler is Func<Event, Task> func => func,
            _ => null
        };

        base.Clear();

        // Found it so save some time.  Return false to
        // short circuit any following calls to html.Append*().
        return false;
    }

    private static Func<Event, Task> Map(Action action) => e =>
    {
        action();
        return Task.CompletedTask;
    };

    private static Func<Event, Task> Map(Action<Event> action) => e =>
    {
        action(e);
        return Task.CompletedTask;
    };

    private static Func<Event, Task> Map(Func<Task> func) => e =>
    {
        return func();
    };

    // Note: NO need to map Func<Event, Task> since it's already the proper signature.
}