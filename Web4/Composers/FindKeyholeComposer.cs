using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Web4.Composers;

public class FindKeyholeComposer : BaseComposer
{
    [ThreadStatic] static FindKeyholeComposer? reusable;
    public static FindKeyholeComposer Shared => reusable ??= new FindKeyholeComposer();

    private readonly KeyCursor keyCursor = new();
    private EventListener eventListener = default;
    private string? key;

    public EventListener FindEventListener(ReadOnlySpan<byte> key, Func<Html> template)
    {
        this.key = Encoding.UTF8.GetString(key);
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
        keyCursor.Reset();
        base.Reset();
    }

    public override bool OnElementBegin(ref Html html)
    {
        keyCursor.MoveNext();
        var key = keyCursor.Current; // TODO: Remove?
        keyCursor.MoveDown();
        return true;
    }

    public override bool OnElementEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        // TODO: Use something else more readable.  It isn't obvious that it's guarding against "tail calls."
        if (key is null)
            return false;

        keyCursor.MoveUp();
        return true;
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

        keyCursor.MoveNext();
        _ = keyCursor.Current; // TODO: Remove?
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
        
        keyCursor.MoveNext();
        if (!keyCursor.Current.SequenceEqual(key))
            return true;

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.Action = listener;
        key = null;
        return false;
    }

    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        keyCursor.MoveNext();
        if (!keyCursor.Current.SequenceEqual(key))
            return true;

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.ActionEvent = listener;
        key = null;
        return false;
    }

    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        keyCursor.MoveNext();
        if (!keyCursor.Current.SequenceEqual(key))
            return true;

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.Func = listener;
        key = null;
        return false;
    }

    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null)
    {
        if (key is null)
            return false;
            
        keyCursor.MoveNext();
        if (!keyCursor.Current.SequenceEqual(key))
            return true;

        // Found it so save some time.  Return false to short circuit any following calls to html.Append*().
        eventListener.FuncEvent = listener;
        key = null;
        return false;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        if (this.key is null)
            return false;
        
        keyCursor.MoveNext();
        var key = keyCursor.Current; // TODO: Remove?
        keyCursor.MoveDown();

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

        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        keyCursor.MoveUp();

        return true;
    }
}