using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using Web4.Composers;

namespace Web4.Composers;

public class FindKeyholeComposer : BaseComposer
{
    private StableKeyTreeWalker keyGenerator = new();
    private string? key = null;
    private Action? action = null;
    private Action<Event>? actionEvent = null;
    private Func<Task>? func = null;
    private Func<Event, Task>? funcEvent = null;

    public EventListener ToEventListenerAndClear(string key, Func<Html> html)
    {
        this.key = key;
        return ToEventListenerAndClear(this, $"{html()}");
    }

    private EventListener ToEventListenerAndClear(BaseComposer composer, [InterpolatedStringHandlerArgument("composer")] Html html)
    {
        var result = new EventListener()
        {
            Action = action,
            ActionEvent = actionEvent,
            Func = func,
            FuncEvent = funcEvent,
        };

        key = null;
        action = null;
        actionEvent = null;
        func = null;
        funcEvent = null;
        Current = null;

        return result;
    }

    public override void OnHtmlPartialBegins(ref Html html)
    {
        if (IsBeforeAppend)
        {
            html.Key = string.Empty;
            keyGenerator.Reset();
            keyGenerator.CreateNewGeneration(string.Empty, html.Length);
        }
        else
        {
            var key = keyGenerator.GetNextKey();
            html.Key = key;
            keyGenerator.CreateNewGeneration(key, html.Length);
        }

        base.OnHtmlPartialBegins(ref html);
    }

    public override bool OnHtmlPartialEnds(ref Html parent, ref Html partial, string? format = null, string? expression = null)
    {
        if (this.key is null)
            return false;

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);

    private bool ToCommonSignatureIfMatch<T>(ref Html parent, T listener)
    {
        if (this.key is null)
            return false;
            
        if (!keyGenerator.IsNextKey(this.key))
            return base.CompleteFormattedValue();

        switch (listener)
        {
            // Example:
            //   void OnClick()
            //   {
            //       Console.WriteLine($"Button was clicked");
            //   }
            case Action a:
                action = a;
                break;

            // Example:
            //   void OnClick(Event e)
            //   {
            //       Console.WriteLine($"Button {e.Target.ID} was clicked");
            //   }
            case Action<Event> ae:
                actionEvent = ae;
                break;

            // Example:
            //   async Task OnClick()
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button was clicked");
            //   }
            case Func<Task> f:
                func = f;
                break;

            // Example:
            //   async Task OnClick(Event e)
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button {e.Target.ID} was clicked");
            //   }
            case Func<Event, Task> fe:
                funcEvent = fe;
                break;
        }

        key = null;
        // Found it so save some time.  Return false to
        // short circuit any following calls to html.Append*().
        return false;
    }

    public override bool WriteMutableValue(ref Html parent, string value) => MoveNextKeyAndComplete();
    public override bool WriteMutableValue(ref Html parent, bool value) => MoveNextKeyAndComplete();
    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null) => MoveNextKeyAndComplete();
    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => MoveNextKeyAndComplete();
    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null) => MoveNextKeyAndComplete();

    public override bool WriteMutableElement<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
    {
        if (this.key is null)
            return false;
            
        var itemCount = partials.Count;
        var key = keyGenerator.GetNextKey();

        keyGenerator.CreateNewGeneration(key, itemCount);

        int i = 0;
        foreach (var partial in partials)
        {
            keyGenerator.ReturnToParent(key, i * 2 - 1, itemCount);
            keyGenerator.MoveNextKey();
            i++;
        }

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }

    private bool MoveNextKeyAndComplete()
    {
        if (this.key is null)
            return false;

        keyGenerator.MoveNextKey();
        return base.CompleteFormattedValue();
    }
}

public static class FindKeyholeComposerExtension
{
    [ThreadStatic]
    static FindKeyholeComposer? current;

    public static EventListener FindEventListener(this Func<Html> html, string key)
    {
        current ??= new FindKeyholeComposer();
        return current.ToEventListenerAndClear(key, html);
    }
}