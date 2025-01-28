using System.Buffers;
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
        this.composer = BaseComposer.Current ??= new DefaultComposer(writer).Init();
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
    public bool AppendFormatted(string value) => WriteMutableValue(value);
    public bool AppendFormatted(bool value) => WriteMutableValue(value);
    public bool AppendFormatted(int value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(long value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(float value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(double value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(decimal value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(DateTime value, string? format = null) => WriteMutableValue(value, format);
    public bool AppendFormatted(TimeSpan value, string? format = null) => WriteMutableValue(value, format);

    private bool WriteMutableValue(string value)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableValue(ref this, value);
        Cursor++;
        return @continue;
    }

    private bool WriteMutableValue(bool value)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableValue(ref this, value);
        Cursor++;
        return @continue;
    }

    private bool WriteMutableValue<T>(T value, string? format = null)
         where T : struct, IUtf8SpanFormattable
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableValue(ref this, value, format);
        Cursor++;
        return @continue;
    }


    // MUTABLE ATTRIBUTES

    // Ex (primary):   <p { style => GetStyle() }>Hello</p>
    // or (ambiguous): <button onclick={ (Event e) => name = e.Target.Name }>Click me</button>
    public bool AppendFormatted(
        Func<Event, string> attribute, 
        string? format = null, 
        [CallerArgumentExpression(nameof(attribute))] string? expression = null) => 
            AppendAmbiguous<string, int>(
                GetArgName(expression), 
                attribute, 
                format: format, 
                expression: expression);
    
    // Ex (primary):   <input type="text" { disabled => isDisabled } />
    // or (ambiguous): <button onclick={ (Event e) => isDisabled = !isDisabled }>Click me</button>
    public bool AppendFormatted(
        Func<Event, bool> attribute, 
        string? format = null, 
        [CallerArgumentExpression(nameof(attribute))] string? expression = null) => 
            AppendAmbiguous<bool, int>(
                GetArgName(expression), 
                attribute, 
                format: format, 
                expression: expression);
    
    // Ex (primary):   <input type="text" { maxlength => c } />
    // or (ambiguous): <button onclick={ (Event e) => c++ }>Click me</button>
    public bool AppendFormatted<T>(
        Func<Event, T> attribute, 
        string? format = null, 
        [CallerArgumentExpression(nameof(attribute))] string? expression = null) 
            where T : struct, IUtf8SpanFormattable => 
            AppendAmbiguous(
                GetArgName(expression), 
                attribute, 
                attribute, 
                format: format, 
                expression: expression);

    // Ex: <h1 { style => $"background-color: { bg }; color: { fg };" }>Hello</h1>
    public bool AppendFormatted(
        Func<Event, Html> attribute, 
        [CallerArgumentExpression(nameof(attribute))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableAttribute(
            ref this, 
            GetArgName(expression), 
            attribute, 
            expression);

        Cursor++;
        return @continue;
    }
    

    // EVENT HANDLERS

    public bool AppendFormatted(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null) => AppendEventListener(listener, format, expression);
    private bool AppendEventListener<T>(T listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = listener switch
        {
            // Ex: <button onclick={ Increment }>Clicks: { c }</button>
            // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
            Action noArg => composer.WriteEventListener(ref this, noArg, format, expression),

            // Ex: <button onclick={ Increment }>Clicks: { c }</button>
            // Ex: <button onclick={ (Event e) => Increment(e) }>Clicks: { c }</button>
            Action<Event> eventArg => composer.WriteEventListener(ref this, eventArg, format, expression),

            // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
            Func<Task> noArgAsync => composer.WriteEventListener(ref this, noArgAsync, format, expression),

            // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
            Func<Event, Task> eventArgAsync => composer.WriteEventListener(ref this, eventArgAsync, format, expression),
            _ => true,
        };
        
        Cursor++;
        return @continue;
    }

    
    // MUTABLE ELEMENTS

    // EX: <div>{ new MyComponent(name: "Rylan") }</div>
    public bool AppendFormatted<TView>(TView view) where TView : IView 
        => AppendFormatted(view.Render());

    // EX: <div>{ MyComponent(content: () => $"<h1>Hello world</h1>")) }</div>
    public bool AppendFormatted(Slot slot) 
        => AppendFormatted(slot());

    // EX: <div>{ user != null ? Avatar(user: user) : SignIn() }</div>
    public bool AppendFormatted(Html html, [CallerArgumentExpression(nameof(html))] string? expression = null) 
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableElement(ref this, html, expression);
        Cursor++;
        return @continue;
    }


    private bool AppendAmbiguous<T, Utf8>(
        ReadOnlySpan<char> argName, 
        Func<Event, T> func, 
        Func<Event, Utf8>? funcUtf8 = null, 
        string? format = null,
        string? expression = null)
            where Utf8 : struct, IUtf8SpanFormattable
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = argName switch
        {
            "e" or "ev" or "evnt" or "@event" or 
            "(e)" or "(ev)" or "(evnt)" or "(@event)"
                => composer.WriteEventListener(
                        ref this, 
                        e => { func(e); }, // Convert signature.  Event handlers always return void.
                        format: format,
                        expression: expression
                    ),
            _ when argName.StartsWith("(Event")
                => composer.WriteEventListener(
                        ref this, 
                        e => { func(e); }, // Convert signature.  Event handlers always return void.
                        format: format,
                        expression: expression
                    ),
            _ when argName.StartsWith("on")
                => throw new ArgumentException($$"""
                    Cannot use the notation <button {onclick => ...}> for event handlers; that's only valid for regular attributes.  Instead, try <button onclick={...}> or <button onclick={() => ...}> or <button onclick={(Event e) => ...}.
                """, expression),
            _ when func is Func<Event, string> funcString
                => composer.WriteMutableAttribute(
                        ref this,
                        attrName: argName, // argName is guaranteed to never be empty
                        attrValue: funcString, // returns string
                        expression: expression
                    ),
            _ when func is Func<Event, bool> funcBool
                => composer.WriteMutableAttribute(
                        ref this,
                        attrName: argName, // argName is guaranteed to never be empty
                        attrValue: funcBool, // returns bool
                        expression: expression
                    ),
            _ when funcUtf8 is not null 
                => composer.WriteMutableAttribute(
                        ref this,
                        attrName: argName, // argName is guaranteed to never be empty
                        attrValue: funcUtf8, // returns int, long, float, double, etc
                        format: format, // All primitives except string and bool are utf8-formattable
                        expression: expression
                    ),
            _ => throw new InvalidOperationException("Html does not support this type."),
        };
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
}