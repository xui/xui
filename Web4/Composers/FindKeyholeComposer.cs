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
        }
        else
        {
            html.Key = keyGenerator.GetNextKey();
        }

        keyGenerator.CreateNewGeneration(html.Key, html.Length);

        base.OnElementBegin(ref html);
    }

    public override bool OnElementEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;

        var cursor = parent.Type != HtmlType.Enumeration ? parent.Cursor : parent.Cursor * 2;
        keyGenerator.ReturnToParent(parent.Key, cursor, parent.Length);

        return base.OnElementEnd(ref parent, html, format, expression);
    }

    public override bool OnStringKeyhole(ref Html parent, string value) => MoveNextKeyAndComplete();
    public override bool OnBoolKeyhole(ref Html parent, bool value) => MoveNextKeyAndComplete();
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => MoveNextKeyAndComplete();
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => MoveNextKeyAndComplete();
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

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        if (this.key is null)
            return false;
        
        htmls.Key = keyGenerator.GetNextKey();
        keyGenerator.CreateNewGeneration(htmls.Key, htmls.Length);
        return true;
    }

    public override bool OnIterate<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        if (this.key is null)
            return false;
        
        foreach (var html in enumerable)
        {
            htmls.AppendFormatted(html);
        }

        return CompleteFormattedValue();
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        keyGenerator.ReturnToParent(parent.Key, parent.Cursor, parent.Length);
        return true;
    }
}