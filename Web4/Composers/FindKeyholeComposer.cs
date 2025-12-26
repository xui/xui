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

    public override void OnElementBegin(ref Html html)
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

        base.OnElementBegin(ref html);
    }

    public override bool OnElementEnd(ref Html parent, scoped Html partial, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;

        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return CompleteFormattedValue();
    }

    public override bool OnString(ref Html parent, string value) => MoveNextKeyAndComplete();
    public override bool OnBool(ref Html parent, bool value) => MoveNextKeyAndComplete();
    public override bool OnInt(ref Html parent, int value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnLong(ref Html parent, long value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnFloat(ref Html parent, float value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDouble(ref Html parent, double value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDecimal(ref Html parent, decimal value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDateTime(ref Html parent, DateTime value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDateOnly(ref Html parent, DateOnly value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnTimeSpan(ref Html parent, TimeSpan value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnTimeOnly(ref Html parent, TimeOnly value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnColor(ref Html parent, Color value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnUri(ref Html parent, Uri value, string? format = null) => MoveNextKeyAndComplete();
    private bool MoveNextKeyAndComplete()
    {
        if (key is null)
            return false;

        keyGenerator.MoveNextKey();
        return CompleteFormattedValue();
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        if (!keyGenerator.IsNextKey(key))
            return CompleteFormattedValue();

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.Action = listener;
        key = null;
        return false;
    }

    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        if (!keyGenerator.IsNextKey(key))
            return CompleteFormattedValue();

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.ActionEvent = listener;
        key = null;
        return false;
    }

    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        if (!keyGenerator.IsNextKey(key))
            return CompleteFormattedValue();

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.Func = listener;
        key = null;
        return false;
    }

    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        if (!keyGenerator.IsNextKey(key))
            return CompleteFormattedValue();

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.FuncEvent = listener;
        key = null;
        return false;
    }

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
}