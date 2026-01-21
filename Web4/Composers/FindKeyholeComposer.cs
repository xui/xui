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
    private bool isFound = false;

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

    // Note: Returning false shortcircuits InterpolatedStringHandler from calling any
    // subsequent AppendFormatted() or AppendLiteral() methods.  
    // And isFound is used to trickle that upwards to all parent Htmls.
    
    protected override bool OnKeyhole()
        => !isFound && base.OnKeyhole();

    public override bool OnHtmlBegin(ref Html html)
        => !isFound && base.OnHtmlBegin(ref html);

    public override bool OnHtmlKeyhole(ref Html parent, scoped Html html, string? format = null, string? expression = null)
        => !isFound && base.OnHtmlKeyhole(ref parent, html, format, expression);

    public override bool OnHtmlEnd(ref Html parent, scoped Html html, string? format = null, string? expression = null)
        => !isFound && base.OnHtmlEnd(ref parent, html, format, expression);

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
        => !isFound && base.OnIteratorBegin(ref parent, ref htmls, format, expression);

    public override bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? format = null, string? expression = null)
        => !isFound && base.OnIteratorKeyhole(ref parent, ref htmls, enumerable, format, expression);

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? format = null, string? expression = null)
        => !isFound && base.OnIteratorEnd(ref parent, ref htmls, format, expression);

    public override bool OnListener(ref Html parent, Action listener, string? format = null, string? expression = null)
    {
        if (isFound)
            return false;
        
        base.OnListener(ref parent, listener, format, expression);

        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.Action = listener;
            isFound = true;
            return false;
        }

        return true;
    }

    public override bool OnListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null)
    {
        if (isFound)
            return false;
            
        base.OnListener(ref parent, listener, format, expression);
        
        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.ActionEvent = listener;
            isFound = true;
            return false;
        }

        return true;
    }

    public override bool OnListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null)
    {
        if (isFound)
            return false;
            
        base.OnListener(ref parent, listener, format, expression);
        
        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.Func = listener;
            isFound = true;
            return false;
        }

        return true;
    }

    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null)
    {
        if (isFound)
            return false;
            
        base.OnListener(ref parent, listener, format, expression);
        
        if (Key.SequenceEqual(searchKey.Span))
        {
            eventListener.FuncEvent = listener;
            isFound = true;
            return false;
        }
        
        return true;
    }

    public override void Reset()
    {
        searchKey = default;
        isFound = false;
        eventListener = default;
        base.Reset();
    }
}