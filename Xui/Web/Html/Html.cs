using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xui.Web.Composers;

namespace Xui.Web;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public ref struct Html
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

    // Ex (primary):   <input type="text" { disabled => isDisabled } />
    // or (ambiguous): <button { onclick => isDisabled = !isDisabled }>Click me</button>
    // or (ambiguous): <button onclick={ e => isDisabled = !isDisabled }>Click me</button>
    public bool AppendFormatted(Func<Event, bool> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) 
        => AppendAmbiguous<bool, int>(GetArgName(expression), attribute, format: null, expression: expression);
    
    // Ex (primary):   <input type="text" { maxlength => c } />
    // or (ambiguous): <button { onclick => c++ }>Click me</button>
    // or (ambiguous): <button onclick={ e => c++ }>Click me</button>
    public bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, [CallerArgumentExpression(nameof(attribute))] string? expression = null) where T : struct, IUtf8SpanFormattable 
        => AppendAmbiguous(GetArgName(expression), attribute, attribute, format: format, expression: expression);

    // Ex: <h1 { style => $"background-color: { bg }; color: { fg };" }>Hello</h1>
    public bool AppendFormatted(Func<Event, Html> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteMutableAttribute(ref this, GetArgName(expression), attribute, expression);
        Cursor++;
        return @continue;
    }
    

    // EVENT HANDLERS

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
    public bool AppendFormatted(Action eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteEventHandler(ref this, eventHandler, expression);
        Cursor++;
        return @continue;
    }
    
    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ e => Increment(e) }>Clicks: { c }</button>
    public bool AppendFormatted(Action<Event> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var argName = GetArgName(expression);
        var @continue = argName switch
        {
            "e" or "ev" or "evnt" or "@event" or 
            "(e)" or "(ev)" or "(evnt)" or "(@event)"
               => composer.WriteEventHandler(ref this, eventHandler, expression),
            "" => composer.WriteEventHandler(ref this, eventHandler, expression),
            _  => composer.WriteEventHandler(ref this, argName, eventHandler, expression),
        };
        Cursor++;
        return @continue;
    }
    
    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteEventHandler(ref this, eventHandler, expression);
        Cursor++;
        return @continue;
    }
    
    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Event, Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
    {
        if (IsEven(Cursor))
            AppendLiteral(string.Empty);

        var @continue = composer.WriteEventHandler(ref this, eventHandler, expression);
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
                => composer.WriteEventHandler(
                        ref this, 
                        // No argName! It's already written from end of the prior string literal (e.g <button onclick=)
                        e => { func(e); }, // make it return void
                        expression: expression
                    ),
            _ when argName.StartsWith("on")
                => composer.WriteEventHandler(
                        ref this, 
                        argName, // Writer is responsible for writing the attribute name (e.g. onclick)
                        () => { func(Event.Empty); }, // make it return void
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