using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MicroHtml.Composers;
using Web4.Dom;

namespace MicroHtml;

public enum HtmlType { Element, Wrapper, Template }

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public ref partial struct Html : IDisposable
{
    [ThreadStatic] static BaseComposer? scopedComposer;
    private readonly BaseComposer composer;

    public int Length { get; private set; }
    public HtmlType Type { get; set; }

    /// <summary>
    /// --- ROOT Html ---
    /// Example:  composer.Compose($"...")
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for creating the root Html.
    /// </summary>
    public Html(int literalLength, int formattedCount, BaseComposer composer)
        : this(composer, literalLength, formattedCount)
    {
        scopedComposer = composer;
    }

    /// <summary>
    /// --- REUSABLE Html (component) ---
    /// Example:  $"...{ MyCustomHtml(c) }..."
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for reusable Html (think components).  
    /// It's relies on ThreadStatic to find its composer (which was established by the root Html).
    /// </summary>
    public Html(int literalLength, int formattedCount, [CallerLineNumber] int relativeOrder = 0)
        : this(scopedComposer ?? throw new NotSupportedException($"This thread's root Html must provide its own composer."), literalLength, formattedCount)
    {
    }

    /// <summary>
    /// --- INLINE Html ---
    /// Example:  $"...{$"...{c}..."}..."
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for inline Html.  It gets its composer from the parent Html.
    /// </summary>
    public Html(int literalLength, int formattedCount, Html parentHtml, out bool @continue, [CallerLineNumber] int relativeOrder = 0)
        : this(parentHtml.composer, literalLength, formattedCount)
    {
        @continue = true;
    }

    private Html(BaseComposer composer, int literalLength, int formattedCount)
    {
        this.composer = composer;
        Length = formattedCount * 2 + 1;
        Type = (literalLength, composer.LiteralLength) switch {
            (0, 0) => HtmlType.Wrapper,
            (> 0, 0) => HtmlType.Template,
            _ => HtmlType.Element
        };

        composer.Grow(ref this, literalLength, formattedCount);

        // e.g. $"".  Complier's lowered code calls no Append*() methods for this use case.
        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }

    private Html(BaseComposer composer, int iteratorCount)
    {
        Length = iteratorCount * 2 + 1;
        Type = HtmlType.Element;
        this.composer = composer;
        // composer.Grow(ref this, 0, iteratorCount);
    }


    // PARTIAL MARKUP
    // Ex (opening): <div id="something"><figure class="bg-slate-100 rounded-xl p-8 dark:bg-slate-800">
    // or (closing): </div></div></div></div></div></div></div>
    public bool AppendLiteral(string literal, [CallerLineNumber] int relativeOrder = 0)
        => composer.OnMarkup(ref this, ref literal, relativeOrder);


    // MUTABLE VALUES
    // Ex: <p>Hello { name }, you have { count } clicks at { DateTime.Now }</p>
    public bool AppendFormatted(string value)
        => composer.OnStringKeyhole(ref this, value);

    public bool AppendFormatted(bool value)
        => composer.OnBoolKeyhole(ref this, value);

    public bool AppendFormatted(int value, string? format = null)
        => composer.OnIntKeyhole(ref this, value, format);

    public bool AppendFormatted(long value, string? format = null)
        => composer.OnLongKeyhole(ref this, value, format);
    
    public bool AppendFormatted(float value, string? format = null)
        => composer.OnFloatKeyhole(ref this, value, format);
    
    public bool AppendFormatted(double value, string? format = null)
        => composer.OnDoubleKeyhole(ref this, value, format);
    
    public bool AppendFormatted(decimal value, string? format = null)
        => composer.OnDecimalKeyhole(ref this, value, format);
    
    public bool AppendFormatted(DateTime value, string? format = null)
        => composer.OnDateTimeKeyhole(ref this, value, format);
    
    public bool AppendFormatted(DateOnly value, string? format = null)
        => composer.OnDateOnlyKeyhole(ref this, value, format);
    
    public bool AppendFormatted(TimeSpan value, string? format = null)
        => composer.OnTimeSpanKeyhole(ref this, value, format);
    
    public bool AppendFormatted(TimeOnly value, string? format = null)
        => composer.OnTimeOnlyKeyhole(ref this, value, format);
    
    public bool AppendFormatted(Color value, string? format = null)
        => composer.OnColorKeyhole(ref this, value, format);

    public bool AppendFormatted(Uri value, string? format = null)
        => composer.OnUriKeyhole(ref this, value, format);


    // EVENT HANDLERS

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
    public bool AppendFormatted(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => composer.OnListener(ref this, listener, format, expression);

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ (Event e) => Increment(e) }>Clicks: { c }</button>
    public bool AppendFormatted(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => composer.OnListener(ref this, listener, format, expression);

    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => composer.OnListener(ref this, listener, format, expression);

    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => composer.OnListener(ref this, listener, format, expression);


    // MUTABLE ELEMENTS

    // EX: <div>{ user != null ? Avatar(user: user) : SignIn() }</div>
    public bool AppendFormatted(
        [InterpolatedStringHandlerArgument("")] scoped Html html, 
        int alignment = -1, 
        string? format = null, 
        [CallerArgumentExpression(nameof(html))] string? expression = null)
    {
        // Possible point of confusion: 
        // By this line, the `scoped Html html` has already set its own keyholes.

        return composer.OnHtmlKeyhole(ref this, html, alignment, format, expression);
    }

    // EX: { names.Select(n => new MyComponent(name: n)) }
    public bool AppendFormatted<T>(
        Html.Enumerable<T> enumerable, 
        string? format = null, 
        [CallerArgumentExpression(nameof(enumerable))] string? expression = null)
    {
        var htmls = new Html(composer, enumerable.Count);
        return composer.OnIteratorKeyhole(ref this, ref htmls, enumerable, format, expression);
    }

    public readonly void Dispose()
    {
        scopedComposer?.Reset();
        scopedComposer = null;
    }
}