using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class FindKeyholeComposer(int key) : BaseComposer
{
    public int Key { get; init; } = key;

    public Func<Event?, Task>? EventHandler { get; set; } = null;

    public override bool WriteEventHandler(ref Html html, Action eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, Action<Event> eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, Func<Task> eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, Func<Event, Task> eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Action eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Action<Event> eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Func<Task> eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);
    public override bool WriteEventHandler(ref Html html, ReadOnlySpan<char> argName, Func<Event, Task> eventHandler, string? expression = null) => ToCommonSignatureIfMatch(ref html, eventHandler);

    private bool ToCommonSignatureIfMatch<T>(ref Html html, T eventHandler)
    {
        if (Cursor != Key)
        {
            return base.CompleteFormattedValue();
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
            _ when eventHandler is Func<Event, Task> func => Map(func),
            _ => null
        };

        base.Clear();

        // Found it so save some time.  Return false to
        // short circuit any following calls to html.Append*().
        return false;
    }

    private static Func<Event?, Task> Map(Action action) => e =>
    {
        action();
        return Task.CompletedTask;
    };

    private static Func<Event?, Task> Map(Action<Event> action) => e =>
    {
        action(e!);
        return Task.CompletedTask;
    };

    private static Func<Event?, Task> Map(Func<Task> func) => e =>
    {
        return func();
    };

    private static Func<Event?, Task> Map(Func<Event, Task> func) => e =>
    {
        return func(e!);
    };
}