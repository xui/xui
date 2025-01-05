using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Xui.Web.Composers;

namespace Xui.Web.HttpX.Composers;

public class FindKeyholeComposer(string key) : BaseComposer
{
    public string Key { get; init; } = key;

    public Func<Event?, Task>? EventHandler { get; set; } = null;

    protected override void Clear()
    {
        Keymaker.Reset(parentKey: string.Empty, cursor: 0);
        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        html.Key = Keymaker.GetNext();
        Keymaker.Reset(parentKey: html.Key, cursor: 0);
        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

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
        if (Keymaker.GetKey(ref html) != Key)
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
            Action action => Map(action),

            // Example:
            //   void OnClick(Event e)
            //   {
            //       Console.WriteLine($"Button {e.currentTarget.id} was clicked");
            //   }
            Action<Event> action => Map(action),

            // Example:
            //   async Task OnClick()
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button was clicked");
            //   }
            Func<Task> func => Map(func),

            // Example:
            //   async Task OnClick(Event e)
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button {e.currentTarget.id} was clicked");
            //   }
            Func<Event, Task> func => Map(func),
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

    public override bool WriteMutableElement(ref Html html, Html partial, string? expression = null)
    {
        Keymaker.Reset(parentKey: html.Key, cursor: html.Cursor / 2 + 1);
        return CompleteFormattedValue();
    }
}