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
    public int Length { get; private set; }
    public bool IsAttribute { get; set; }

    public Html(int literalLength, int formattedCount)
    {
        Length = 2 * formattedCount + 1;
        // For now, do not allow the creation of Html instances detached from the root-node.
        this.composer = BaseComposer.Current ?? throw new ArgumentNullException("BaseComposer.Current");
        this.composer.Grow(ref this, literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, BaseComposer composer)
    {
        Length = 2 * formattedCount + 1;
        this.composer = BaseComposer.Current ??= composer.Init();
        this.composer.Grow(ref this, literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        Length = 2 * formattedCount + 1;
        this.composer = BaseComposer.Current ??= new HtmlComposer(writer).Init();
        this.composer.Grow(ref this, literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer, StreamingComposer composer)
    {
        Length = 2 * formattedCount + 1;
        composer.Writer = writer;
        this.composer = BaseComposer.Current ??= composer.Init();
        this.composer.Grow(ref this, literalLength, formattedCount);
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

    // EX: <div>{ new MyComponent(name: "Rylan") }</div>
    public bool AppendFormatted<TComponent>(TComponent component, string? format = null, [CallerArgumentExpression(nameof(component))] string? expression = null) where TComponent : struct, IComponent
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableElement(ref this, ref component, format, expression);
        Cursor++;
        return @continue;
    }

    // EX: <div>{ user != null ? Avatar(user: user) : SignIn() }</div>
    public bool AppendFormatted(Html html, string? format = null, [CallerArgumentExpression(nameof(html))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.OnHtmlPartialEnds(ref this, ref html, format, expression);
        Cursor++;
        return @continue;
    }

    // EX: { names.Select(n => new MyComponent(name: n)) }
    public bool AppendFormatted<T>(Html.Enumerable<T> htmls, string? format = null, [CallerArgumentExpression(nameof(htmls))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableElement(ref this, htmls, format, expression);
        Cursor++;

        return @continue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(int number) => number % 2 == 0;

    private static ReadOnlySpan<char> GetArgName(ReadOnlySpan<char> expression)
    {
        // Receives the expression but passed in as a string literal thanks to the
        // compile-time magic of CompilerServices and CallerArgumentExpression!
        // It can then peek at the first handful of bytes to disambiguate 
        // the developer's intended usage based on "convention" (i.e the arg's name)
        // Example expressions:
        //     "maxlength => size" (this is an attribute)
        //     "e => c++"          (this is an event handler method that takes an event as an argument)
        //     "onclick => c++"    (this is an event handler method that takes zero arguments)

        if (expression.Length >= 2)
        {
            // TODO: Make sure this doesn't allocate.
            var end = expression.IndexOfAny([' ', '=']);
            if (end > 0)
            {
                var start = expression[0] == '@' ? 1 : 0;
                return expression[start..end];
            }
        }
        return string.Empty;
    }

    public struct Enumerable<T>(IEnumerable<T> source, Func<T, Html> selector)
    {
        public readonly int Count => source.Count();
        public readonly Enumerator<T> GetEnumerator() => new(source, selector);
    }

    public struct Enumerator<T>(IEnumerable<T> source, Func<T, Html> selector)
    {
        int index = -1;
        readonly IEnumerator<T>? enumerator = source is not IList<T> ? source.GetEnumerator() : null;

        public readonly Html Current => source switch
        {
            IList<T> list => selector(list[index]),
            _ => selector(enumerator!.Current),
        };

        public bool MoveNext() => source switch
        {
            IList<T> list => ++index < list.Count,
            _ => enumerator!.MoveNext(),
        };
    }
}