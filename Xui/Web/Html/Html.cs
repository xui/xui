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
        composer.PrependDynamicElement();
    }

    public Html(int literalLength, int formattedCount, BaseComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Grow(literalLength, formattedCount);
        composer.PrependDynamicElement();
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
    {
        this.composer = BaseComposer.Current ??= new DefaultComposer(writer);
        composer.Grow(literalLength, formattedCount);
        composer.PrependDynamicElement();
    }

    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer, StreamingComposer composer)
    {
        this.composer = BaseComposer.Current ??= composer;
        composer.Writer = writer;
        composer.Grow(literalLength, formattedCount);
        composer.PrependDynamicElement();
    }

    // PARTIAL MARKUP
    // Ex (opening): <div id="something"><figure class="bg-slate-100 rounded-xl p-8 dark:bg-slate-800">
    // or (closing): </div></div></div></div></div></div></div>
    public readonly bool AppendLiteral(string literal) => composer.AppendStaticPartialMarkup(literal);


    // DYNAMIC VALUES
    // Ex: <p>Hello { name }, you have { count } clicks at { DateTime.Now }</p>
    public readonly bool AppendFormatted(string value) => composer.AppendDynamicValue(value);
    public readonly bool AppendFormatted(bool value) => composer.AppendDynamicValue(value);
    public readonly bool AppendFormatted(int value, string? format = null) => composer.AppendDynamicValue(value, format);
    public readonly bool AppendFormatted(long value, string? format = null) => composer.AppendDynamicValue(value, format);
    public readonly bool AppendFormatted(float value, string? format = null) => composer.AppendDynamicValue(value, format);
    public readonly bool AppendFormatted(double value, string? format = null) => composer.AppendDynamicValue(value, format);
    public readonly bool AppendFormatted(decimal value, string? format = null) => composer.AppendDynamicValue(value, format);
    public readonly bool AppendFormatted(DateTime value, string? format = null) => composer.AppendDynamicValue(value, format);
    public readonly bool AppendFormatted(TimeSpan value, string? format = null) => composer.AppendDynamicValue(value, format);


    // DYNAMIC ATTRIBUTES

    // Ex (primary):   <input type="text" { disabled => isDisabled } />
    // or (ambiguous): <button { onclick => isDisabled = !isDisabled }>Click me</button>
    // or (ambiguous): <button onclick={ e => isDisabled = !isDisabled }>Click me</button>
    public readonly bool AppendFormatted(Func<Event, bool> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) 
        => AppendAmbiguous<bool, int>(GetArgName(expression), attribute);
    
    // Ex (primary):   <input type="text" { maxlength => c } />
    // or (ambiguous): <button { onclick => c++ }>Click me</button>
    // or (ambiguous): <button onclick={ e => c++ }>Click me</button>
    public readonly bool AppendFormatted<T>(Func<Event, T> attribute, string? format = null, [CallerArgumentExpression(nameof(attribute))] string? expression = null) where T : struct, IUtf8SpanFormattable 
        => AppendAmbiguous(GetArgName(expression), attribute, attribute, format);

    // Ex: <h1 { style => $"background-color: { bg }; color: { fg };" }>Hello</h1>
    public readonly bool AppendFormatted(Func<string, Html> attribute, [CallerArgumentExpression(nameof(attribute))] string? expression = null) 
        => composer.AppendDynamicAttribute(GetArgName(expression), attribute);
    

    // EVENT HANDLERS
    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Action eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(string.Empty, eventHandler);
    
    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ e => Increment(e) }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Action<Event> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(GetArgName(expression), eventHandler);
    
    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Func<Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(string.Empty, eventHandler);
    
    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public readonly bool AppendFormatted(Func<Event, Task> eventHandler, [CallerArgumentExpression(nameof(eventHandler))] string? expression = null)
        => composer.AppendEventHandler(string.Empty, eventHandler);
    
    
    // DYNAMIC ELEMENTS
    // EX: <div>{ new MyComponent(name: "Rylan") }</div>
    public readonly bool AppendFormatted<TView>(TView view) where TView : IView => composer.AppendDynamicElement(view.Render());
    // EX: <div>{ MyComponent(content: () => $"<h1>Hello world</h1>")) }</div>
    public readonly bool AppendFormatted(Slot slot) => composer.AppendDynamicElement(slot());
    // EX: <div>{ user != null ? Avatar(user: user) : SignIn() }</div>
    public readonly bool AppendFormatted(Html html) => composer.AppendDynamicElement(html);


    private bool AppendAmbiguous<T, Utf8>(
        ReadOnlySpan<char> argName, 
        Func<Event, T> func, 
        Func<Event, Utf8>? funcUtf8 = null, 
        string? format = null)
            where Utf8 : IUtf8SpanFormattable
        => argName switch
        {
            "e" or "ev" or "evnt" or "@event" or 
            "(e)" or "(ev)" or "(evnt)" or "(@event)"
                => composer.AppendEventHandler(
                        string.Empty, 
                        e => { func(e); } // make it return void
                    ),
            _ when argName.StartsWith("on")
                => composer.AppendEventHandler(
                        argName, 
                        () => { func(Event.Empty); } // make it return void
                    ),
            _ when func is Func<Event, bool> funcBool
                => composer.AppendDynamicAttribute(
                        attrName: argName, 
                        attrValue: funcBool // returns bool
                    ),
            _ when funcUtf8 is not null 
                => composer.AppendDynamicAttribute(
                        attrName: argName, 
                        attrValue: funcUtf8, // returns int, long, float, double, etc
                        format: format
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