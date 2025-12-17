using System.Buffers;
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
        : this(literalLength, formattedCount, -1, composer.Init())
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

    // Inline Html: $"<div>{ GetHtml() }</div>"
    public Html(int literalLength, int formattedCount, [CallerLineNumber] int lineNumber = 0)
        : this(literalLength, formattedCount, lineNumber, BaseComposer.Current ?? throw new NotSupportedException($"This thread's root Html must provide its own composer."))
    {
        // How very interesting!  This ctor performs faster than the one above.
        // Evidently referencing the composer via ThreadStatic (BaseComposer.Current) 
        // incurs less penalty than passing it through the constructor via the parent Html.
        //
        // | Method               | Mean      | Error     | StdDev    | Op/s          | Allocated |
        // |--------------------- |----------:|----------:|----------:|--------------:|----------:|
        // | InlineAsInterpolated | 10.178 ns | 0.9530 ns | 0.0522 ns |  98,246,787.3 |         - |
        // | InlineAsMethod       |  9.887 ns | 1.1004 ns | 0.0603 ns | 101,146,868.6 |         - |
    }

    private Html(int literalLength, int formattedCount, int relativeOrder, BaseComposer composer)
    {
        Length = 2 * formattedCount + 1;
        RelativeOrder = relativeOrder;

        this.composer = composer;
        this.composer.Grow(literalLength, formattedCount);
        this.composer.OnHtmlPartialBegins(ref this);

        // e.g. $"".  Complier's lowered code calls no Append*() methods for this use case.
        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }


    // PARTIAL MARKUP
    // Ex (opening): <div id="something"><figure class="bg-slate-100 rounded-xl p-8 dark:bg-slate-800">
    // or (closing): </div></div></div></div></div></div></div>
    public bool AppendLiteral(string literal)
    {
        var @continue = composer.WriteImmutableMarkup(ref this, literal);
        Cursor++;
        return @continue;
    }


    // MUTABLE VALUES
    // Ex: <p>Hello { name }, you have { count } clicks at { DateTime.Now }</p>
    public bool AppendFormatted(string value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(bool value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(Color value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(Uri value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(int value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(long value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(float value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(double value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(decimal value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(DateTime value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(DateOnly value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(TimeSpan value, string? format = null) => WriteMutableValueUtf8(value, format);
    public bool AppendFormatted(TimeOnly value, string? format = null) => WriteMutableValueUtf8(value, format);

    private bool WriteMutableValue<T>(T value, string? format = null)
    {
        // TODO: Faster without the switch?  Benchmark to confirm.

        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = value switch
        {
            string s => composer.WriteMutableValue(ref this, s),
            bool b => composer.WriteMutableValue(ref this, b),
            Color c => composer.WriteMutableValue(ref this, c, format),
            Uri u => composer.WriteMutableValue(ref this, u, format),
            _ => true
        };

        Cursor++;
        return @continue;
    }

    private bool WriteMutableValueUtf8<T>(T value, string? format = null)
         where T : struct, IUtf8SpanFormattable
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableValue(ref this, value, format);

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

        var @continue = composer.WriteEventListener(ref this, listener, format, expression);
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

        var @continue = composer.WriteEventListener(ref this, listener, format, expression);
        Cursor++;
        return @continue;
    }

    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteEventListener(ref this, listener, format, expression);
        Cursor++;
        return @continue;
    }

    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteEventListener(ref this, listener, format, expression);
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

        var @continue = composer.OnHtmlPartialEnds(ref this, html, format, expression);
        Cursor++;
        return @continue;
    }

    // EX: { names.Select(n => new MyComponent(name: n)) }
    public bool AppendFormatted<T>(Html.Enumerable<T> htmls, string? format = null, [CallerArgumentExpression(nameof(htmls))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableNode(ref this, htmls, format, expression);
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