using System.Drawing;
using System.Runtime.CompilerServices;

namespace Web4.Composers;

public class FindKeyholeComposer : KeyholeComposer
{
    [ThreadStatic] static FindKeyholeComposer? reusable;
    public static FindKeyholeComposer Shared => reusable ??= new FindKeyholeComposer();

    private EventListener eventListener = default;
    private byte[] keyBuffer = [];
    private Memory<byte> searchKey;
    private bool shortCircuitTailCalls = false;

    public EventListener FindEventListener(ReadOnlySpan<byte> key, Func<Html> template)
    {
        if (keyBuffer.Length < key.Length)
            keyBuffer = new byte[key.Length];
        key.CopyTo(keyBuffer);
        return FindEventListener(keyBuffer.AsMemory(..key.Length), template);
    }

    public EventListener FindEventListener(Memory<byte> key, Func<Html> template)
    {
        searchKey = key;
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
    
    public override bool OnHtmlEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
        return base.OnHtmlEnd(ref parent, html, format, expression);;
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
        if (shortCircuitTailCalls)
            return false;
        return base.OnKeyhole();
    }

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
        
        base.OnListener(ref parent, listener, format, expression);

        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.Action = listener;
            shortCircuitTailCalls = true;
            // Found it so save some time: return false to short circuit any following calls to this html.Append*().
            return false;
        }
        return true;
    }

    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
            
        base.OnListener(ref parent, listener, format, expression);
        
        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.ActionEvent = listener;
            shortCircuitTailCalls = true;
            // Found it so save some time: return false to short circuit any following calls to this html.Append*().
            return false;
        }
        return true;
    }

    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
            
        base.OnListener(ref parent, listener, format, expression);
        
        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.Func = listener;
            shortCircuitTailCalls = true;
            // Found it so save some time: return false to short circuit any following calls to this html.Append*().
            return false;
        }
        return true;
    }

    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
            
        base.OnListener(ref parent, listener, format, expression);
        
        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.FuncEvent = listener;
            shortCircuitTailCalls = true;
            // Found it so save some time: return false to short circuit any following calls to this html.Append*().
            return false;
        }
        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
        
        return base.OnIteratorBegin(ref parent, ref htmls, format, expression);
    }

    public override bool OnIterate<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;
        
        foreach (var html in enumerable)
            htmls.AppendFormatted(html);

        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
    {
        if (shortCircuitTailCalls)
            return false;

        return base.OnIteratorEnd(ref parent, ref htmls, format, expression);
    }

    public override void Reset()
    {
        searchKey = default;
        shortCircuitTailCalls = false;
        eventListener = default;
        base.Reset();
    }
}