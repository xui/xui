using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using Web4.Composers;

namespace Web4.Composers;

public class FindKeyholeComposer(string key) : BaseComposer
{
    private string parentKey = string.Empty;
    private int parentLength = 0;
    private int cursor = 0;
    private Action? action = null;
    private Action<Event>? actionEvent = null;
    private Func<Task>? func = null;
    private Func<Event, Task>? funcEvent = null;

    public string Key { get; init; } = key;

    public EventListener Listener { get => new()
    {
        Action = action, 
        ActionEvent = actionEvent, 
        Func = func, 
        FuncEvent = funcEvent,
    };}

    protected override void Clear()
    {
        parentKey = string.Empty;
        parentLength = 0;
        cursor = 0;

        base.Clear();
    }

    public override void PrepareHtml(ref Html html, int literalLength, int formattedCount)
    {
        // Skip the root.  It doesn't need a key.
        html.Key = IsInitialAppend()
            ? string.Empty
            : Keymaker.GetKey(parentKey, cursor++, parentLength);
        parentKey = html.Key;
        parentLength = html.Length;
        cursor = 0;

        base.PrepareHtml(ref html, literalLength, formattedCount);
    }

    public override bool WriteEventListener(ref Html parent, Action listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, Action<Event> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, Func<Task> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, Func<Event, Task> listener, string? format = null, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);
    public override bool WriteEventListener(ref Html parent, ReadOnlySpan<char> argName, Action<object> listener, string? expression = null) => ToCommonSignatureIfMatch(ref parent, listener);

    private bool ToCommonSignatureIfMatch<T>(ref Html parent, T listener)
    {
        var key = Keymaker.GetKey(parentKey, cursor++, parent.Length);
        if (key != Key)
        {
            return base.CompleteFormattedValue();
        }

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
        };

        // TODO: Shortcircuiting seems to leak memory.  But where?  For now, keep the full flow.
        // Test this by perf-surrounding `var keyhole = window.GetKeyhole(key);` in HttpXContext.ListenForEvents.
        return base.CompleteFormattedValue();

        // // Found it so save some time.  Return false to
        // // short circuit any following calls to html.Append*().
        // Clear();
        // return false;
    }

    public override bool WriteMutableElement(ref Html parent, Html partial, string? expression = null)
    {
        parentKey = parent.Key;
        parentLength = parent.Length;
        cursor = parent.Cursor / 2 + 1;
        return CompleteFormattedValue();
    }



    // TODO: TEMPORARY!!!!!!!  Perhaps the cursor should always move itself at the base level?

    private FindKeyholeComposer IncrementCursor()
    {
        cursor++;
        return this;
    }

    public override bool WriteMutableValue(ref Html parent, string value) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableValue(ref Html parent, bool value) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableValue(ref Html parent, Color value, string? format = null) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableValue(ref Html parent, Uri value, string? format = null) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableValue<T>(ref Html parent, T value, string? format = null) => IncrementCursor().CompleteFormattedValue();
    
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, string> attrValue, string? expression = null) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, bool> attrValue, string? expression = null) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableAttribute<T>(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, T> attrValue, string? format = null, string? expression = null) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableAttribute(ref Html parent, ReadOnlySpan<char> attrName, Func<Event, Html> attrValue, string? expression = null) => IncrementCursor().CompleteFormattedValue();

    public override bool WriteMutableElement<TView>(ref Html parent, TView view) => IncrementCursor().CompleteFormattedValue();
    public override bool WriteMutableElement(ref Html parent, Slot slot) => IncrementCursor().CompleteFormattedValue();
}