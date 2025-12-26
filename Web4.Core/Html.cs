using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Web4.Composers;

namespace Web4;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public ref partial struct Html : IDisposable
{
    [ThreadStatic]
    private static BaseComposer? scopedComposer;
    private readonly BaseComposer composer;

    public string Key { get; set; }
    public int Index { get; set; }
    public int Cursor { get; private set; }
    public int Length { get; private set; } // TODO: Rename to `Segments` or `KeyCount`?
    public bool IsAttribute { get; set; } // TODO: Rename to `SuppressSentinels` or something closer to its purpose?
    public int RelativeOrder { get; private set; }

    /// <summary>
    /// --- ROOT Html ---
    /// Example:  composer.Compose($"...")
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for creating the root Html.
    /// </summary>
    public Html(int literalLength, int formattedCount, BaseComposer composer)
        : this(literalLength, formattedCount, -1, composer)
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
        : this(literalLength, formattedCount, relativeOrder, scopedComposer ?? throw new NotSupportedException($"This thread's root Html must provide its own composer."))
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
        : this(literalLength, formattedCount, relativeOrder, parentHtml.composer)
    {
        @continue = true;
    }

    private Html(int literalLength, int formattedCount, int relativeOrder, BaseComposer composer)
    {
        Length = 2 * formattedCount + 1;
        RelativeOrder = relativeOrder;

        this.composer = composer;
        this.composer.Grow(literalLength, formattedCount);
        this.composer.OnElementBegin(ref this);

        // e.g. $"".  Complier's lowered code calls no Append*() methods for this use case.
        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }


    // PARTIAL MARKUP
    // Ex (opening): <div id="something"><figure class="bg-slate-100 rounded-xl p-8 dark:bg-slate-800">
    // or (closing): </div></div></div></div></div></div></div>
    public bool AppendLiteral(string literal)
    {
        var @continue = composer.OnMarkup(ref this, literal);
        Cursor++;
        return @continue;
    }


    // MUTABLE VALUES
    // Ex: <p>Hello { name }, you have { count } clicks at { DateTime.Now }</p>
    public bool AppendFormatted(string value)
    {
        // Note: String doesn't implement IUtf8SpanFormattable
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnStringKeyhole(ref this, value);
        Cursor++;
        return @continue;
    }

    public bool AppendFormatted(bool value)
    {
        // Note: Boolean is a struct but it doesn't implement IUtf8SpanFormattable
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnBoolKeyhole(ref this, value);
        Cursor++;
        return @continue;
    }

    public bool AppendFormatted(int value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnIntKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }

    public bool AppendFormatted(long value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnLongKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(float value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnFloatKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(double value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnDoubleKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(decimal value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnDecimalKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(DateTime value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnDateTimeKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(DateOnly value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnDateOnlyKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(TimeSpan value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnTimeSpanKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(TimeOnly value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnTimeOnlyKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }
    
    public bool AppendFormatted(Color value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnColorKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }

    public bool AppendFormatted(Uri value, string? format = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnUriKeyhole(ref this, value, format);
        Cursor++;
        return @continue;
    }


    // EVENT HANDLERS

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
    public bool AppendFormatted(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnListener(ref this, listener, format, expression);
        Cursor++;
        return @continue;
    }

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ (Event e) => Increment(e) }>Clicks: { c }</button>
    public bool AppendFormatted(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnListener(ref this, listener, format, expression);
        Cursor++;
        return @continue;
    }

    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnListener(ref this, listener, format, expression);
        Cursor++;
        return @continue;
    }

    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnListener(ref this, listener, format, expression);
        Cursor++;
        return @continue;
    }


    // MUTABLE ELEMENTS

    // EX: <div>{ user != null ? Avatar(user: user) : SignIn() }</div>
    public bool AppendFormatted([InterpolatedStringHandlerArgument("")] scoped Html html, int alignment = -1, string? format = null, [CallerArgumentExpression(nameof(html))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);
        
        if (alignment >= 0)
            html.RelativeOrder = alignment;

        var @continue = composer.OnElementEnd(ref this, html, format, expression);
        Cursor++;
        return @continue;
    }

    // EX: { names.Select(n => new MyComponent(name: n)) }
    public bool AppendFormatted<T>(Html.Enumerable<T> htmls, string? format = null, [CallerArgumentExpression(nameof(htmls))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnIterate(ref this, htmls, format, expression);
        Cursor++;
        return @continue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEven(int number) => number % 2 == 0;

    public readonly void Dispose()
    {
        scopedComposer?.Reset();
        scopedComposer = null;
    }
}