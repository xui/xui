using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public class FindKeyholeComposer : BaseComposer
{
    [ThreadStatic] static FindKeyholeComposer? reusable;
    public static FindKeyholeComposer Shared => reusable ??= new FindKeyholeComposer();

    private StableKeyTreeWalker keyGenerator = new();
    private EventListener eventListener = default;
    private string? key;

    public EventListener FindEventListener(string key, Func<Html> template)
    {
        this.key = key;
        return Interpolate($"{template()}");
    }

    private EventListener Interpolate([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the templating work has already completed.
        
        // Hang onto the result before html.Dispose() resets this class.
        var result = eventListener;

        // html.Dispose() calls composer.Reset() which sets everything back to null.
        html.Dispose();

        // Do something interesting with the result.
        return result;
    }
    
    public override void Reset()
    {
        key = null;
        eventListener = default;
        base.Reset();
    }

    public override void OnElementBegins(ref Html html)
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

        base.OnElementBegins(ref html);
    }

    public override bool OnElementEnds(ref Html parent, scoped Html partial, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);

    private bool ToCommonSignatureIfMatch<T>(ref Html parent, T listener)
    {
        if (key is null)
            return false;
            
        if (!keyGenerator.IsNextKey(key))
            return CompleteFormattedValue();

        switch (listener)
        {
            // Example:
            //   void OnClick()
            //   {
            //       Console.WriteLine($"Button was clicked");
            //   }
            case Action a:
                eventListener.Action = a;
                break;

            // Example:
            //   void OnClick(Event e)
            //   {
            //       Console.WriteLine($"Button {e.Target.ID} was clicked");
            //   }
            case Action<Event> ae:
                eventListener.ActionEvent = ae;
                break;

            // Example:
            //   async Task OnClick()
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button was clicked");
            //   }
            case Func<Task> f:
                eventListener.Func = f;
                break;

            // Example:
            //   async Task OnClick(Event e)
            //   {
            //       await Task.Delay(1000);
            //       Console.WriteLine($"Button {e.Target.ID} was clicked");
            //   }
            case Func<Event, Task> fe:
                eventListener.FuncEvent = fe;
                break;
        }

        key = null;
        // Found it so save some time.  Return false to
        // short circuit any following calls to html.Append*().
        return false;
    }

    public override bool OnString(ref Html parent, string value) => MoveNextKeyAndComplete();
    public override bool OnBool(ref Html parent, bool value) => MoveNextKeyAndComplete();
    public override bool OnColor(ref Html parent, Color value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnUri(ref Html parent, Uri value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnValue<T>(ref Html parent, T value, string? format = null) => MoveNextKeyAndComplete();

    public override bool OnIterate<T>(ref Html parent, Html.Enumerable<T> partials, string? format = null, string? expression = null)
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
        if (key is null)
            return false;

        keyGenerator.MoveNextKey();
        return CompleteFormattedValue();
    }
}