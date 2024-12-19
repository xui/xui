using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xui.Web.Composers;

namespace Xui.Web;

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public readonly ref struct Html
{
    readonly BaseComposer composer;

    public Html(int literalLength, int formattedCount)
    {
        // For now, do not allow the creation of Html instances detached from the root-node.
        this.composer = BaseComposer.Current ?? throw new ArgumentNullException("BaseComposer.Current");
        composer.Grow(literalLength, formattedCount);
        composer.PrepareHtml(literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, BaseComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Grow(literalLength, formattedCount);
        composer.PrepareHtml(literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        this.composer = BaseComposer.Current ??= new DefaultComposer(writer);
        composer.Grow(literalLength, formattedCount);
        composer.PrepareHtml(literalLength, formattedCount);
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer, StreamingComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Writer = writer;
        composer.Grow(literalLength, formattedCount);
        composer.PrepareHtml(literalLength, formattedCount);
    }

    // PARTIAL MARKUP
    // Ex (opening): <div id="something"><figure class="bg-slate-100 rounded-xl p-8 dark:bg-slate-800">
    // or (closing): </div></div></div></div></div></div></div>
    public readonly bool AppendLiteral(string literal) => composer.AppendImmutableMarkup(literal);


    // MUTABLE VALUES
    // Ex: <p>Hello { name }, you have { count } clicks at { DateTime.Now }</p>
    public readonly bool AppendFormatted(string value) => composer.AppendMutableValue(value);
    public readonly bool AppendFormatted(bool value) => composer.AppendMutableValue(value);
    public readonly bool AppendFormatted(int value, string? format = null) => composer.AppendMutableValue(value, format);
    public readonly bool AppendFormatted(long value, string? format = null) => composer.AppendMutableValue(value, format);
    public readonly bool AppendFormatted(float value, string? format = null) => composer.AppendMutableValue(value, format);
    public readonly bool AppendFormatted(double value, string? format = null) => composer.AppendMutableValue(value, format);
    public readonly bool AppendFormatted(decimal value, string? format = null) => composer.AppendMutableValue(value, format);
    public readonly bool AppendFormatted(DateTime value, string? format = null) => composer.AppendMutableValue(value, format);
    public readonly bool AppendFormatted(TimeSpan value, string? format = null) => composer.AppendMutableValue(value, format);


    // MUTABLE ATTRIBUTES

    // Ex (primary):   <input type="text" { disabled => isDisabled } />
    // or (ambiguous): <button { onclick => isDisabled = !isDisabled }>Click me</button>
    // or (ambiguous): <button onclick={ e => isDisabled = !isDisabled }>Click me</button>
    public readonly bool AppendFormatted(Func<Event, bool> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) 
        => AppendAmbiguous<bool, int>(GetArgName(expression), attribute, format: null, expression: expression);
    
    // Ex (primary):   <input type="text" { maxlength => c } />
    // or (ambiguous): <button { onclick => c++ }>Click me</button>
    // or (ambiguous): <button onclick={ e => c++ }>Click me</button>
    public readonly bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, [CallerArgumentExpression(nameof(attribute))] string? expression = null) where T : struct, IUtf8SpanFormattable 
        => AppendAmbiguous(GetArgName(expression), attribute, attribute, format: format, expression: expression);

    // Ex: <h1 { style => $"background-color: { bg }; color: { fg };" }>Hello</h1>
    public readonly bool AppendFormatted(Func<string, Html> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) 
        => composer.AppendMutableAttribute(GetArgName(expression), attribute, expression);
    

    // EVENT HANDLERS
    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Action eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(eventHandler, expression);
    
    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ e => Increment(e) }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Action<Event> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => AppendEventHandler(GetArgName(expression), eventHandler, expression);
    
    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Func<Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(eventHandler, expression);
    
    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Func<Event, Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(eventHandler, expression);
    
    private readonly bool AppendEventHandler(ReadOnlySpan<char> argName, Action<Event> eventHandler, string? expression = null)
        => argName switch
        {
            "e" or "ev" or "evnt" or "@event" or 
            "(e)" or "(ev)" or "(evnt)" or "(@event)"
               => composer.AppendEventHandler(eventHandler, expression),
            "" => composer.AppendEventHandler(eventHandler, expression),
            _  => composer.AppendEventHandler(argName, eventHandler, expression),
        };

    
    // MUTABLE ELEMENTS
    // EX: <div>{ new MyComponent(name: "Rylan") }</div>
    public readonly bool AppendFormatted<TView>(TView view) where TView : IView 
        => composer.AppendMutableElement(view.Render());
    // EX: <div>{ MyComponent(content: () => $"<h1>Hello world</h1>")) }</div>
    public readonly bool AppendFormatted(Slot slot) 
        => composer.AppendMutableElement(slot());
    // EX: <div>{ user != null ? Avatar(user: user) : SignIn() }</div>
    public readonly bool AppendFormatted(Html html, [CallerArgumentExpression(nameof(html))] string? expression = null) 
        => composer.AppendMutableElement(html, expression);


    private bool AppendAmbiguous<T, Utf8>(
        ReadOnlySpan<char> argName, 
        Func<Event, T> func, 
        Func<Event, Utf8>? funcUtf8 = null, 
        string? format = null,
        string? expression = null)
            where Utf8 : struct, IUtf8SpanFormattable
        => argName switch
        {
            "e" or "ev" or "evnt" or "@event" or 
            "(e)" or "(ev)" or "(evnt)" or "(@event)"
                => composer.AppendEventHandler(
                        // No argName! It's already written from end of the prior string literal (e.g <button onclick=)
                        e => { func(e); }, // make it return void
                        expression: expression
                    ),
            _ when argName.StartsWith("on")
                => composer.AppendEventHandler(
                        argName, // Writer is responsible for writing the attribute name (e.g. onclick)
                        () => { func(Event.Empty); }, // make it return void
                        expression: expression
                    ),
            _ when func is Func<Event, bool> funcBool
                => composer.AppendMutableAttribute(
                        attrName: argName, // argName is guaranteed to never be empty
                        attrValue: funcBool, // returns bool
                        expression: expression
                    ),
            _ when funcUtf8 is not null 
                => composer.AppendMutableAttribute(
                        attrName: argName, // argName is guaranteed to never be empty
                        attrValue: funcUtf8, // returns int, long, float, double, etc
                        format: format, // All primitives except string and bool are utf8-formattable
                        expression: expression
                    ),
            _ => throw new InvalidOperationException("Html does not support this type."),
        };

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