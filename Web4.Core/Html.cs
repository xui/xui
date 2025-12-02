using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Web4.Composers;

namespace Web4;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public ref partial struct Html
{
    readonly BaseComposer composer;
    public string Key { get; set; }
    public int Index { get; set; }
    public int Cursor { get; private set; }
    public int Length { get; private set; } // TODO: Rename to `Segments` or `KeyCount`?
    public bool IsAttribute { get; set; }
    public int RelativeOrder { get; private set; }

    public Html(int literalLength, int formattedCount, [CallerLineNumber] int lineNumber = 0)
    {
        Length = 2 * formattedCount + 1;
        RelativeOrder = lineNumber;

        // For now, do not allow the creation of Html instances detached from the root-node.
        this.composer = BaseComposer.Current ?? throw new ArgumentNullException("BaseComposer.Current");
        this.composer.Grow(ref this, literalLength, formattedCount);
        this.composer.OnHtmlPartialBegins(ref this);

        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }

    public Html(int literalLength, int formattedCount, Html html, out bool @continue, [CallerLineNumber] int lineNumber = 0)
    {
        Length = 2 * formattedCount + 1;
        RelativeOrder = lineNumber;

        this.composer = html.composer;
        this.composer.Grow(ref this, literalLength, formattedCount);
        this.composer.OnHtmlPartialBegins(ref this);

        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
        
        @continue = true;
    }

    public Html(int literalLength, int formattedCount, BaseComposer composer)
    {
        Length = 2 * formattedCount + 1;
        this.composer = BaseComposer.Current ??= composer.Init();
        this.composer.Grow(ref this, literalLength, formattedCount);
        this.composer.OnHtmlPartialBegins(ref this);

        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        Length = 2 * formattedCount + 1;
        this.composer = BaseComposer.Current ??= new HtmlComposer(writer).Init();
        this.composer.Grow(ref this, literalLength, formattedCount);
        this.composer.OnHtmlPartialBegins(ref this);

        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer, StreamingComposer composer)
    {
        Length = 2 * formattedCount + 1;
        composer.Writer = writer;
        this.composer = BaseComposer.Current ??= composer.Init();
        this.composer.Grow(ref this, literalLength, formattedCount);
        this.composer.OnHtmlPartialBegins(ref this);

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
}