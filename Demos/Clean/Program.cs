using System.Collections;
using Web4;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string name = "World";
int c = 0;
double d = 3.14;
string[] names = ["one", "two", "three", "four", "five", "six", "seven"];

var window = app.MapWeb4("/app", () => $"""
    <html>
        <body>
            <button onclick={() => c++}>
                Clicks: {c}
            </button>
            {c:c} and {d:c}

            {$"one {"two"} {3} four"}
            {MyButton(text: "Nice to repeat you")}
            {MyButton(text: "Nice to repeat you"):Fade}

            <br />
            <br />
            {MyButton(text: "111")}{MyButton(text: "222")}{MyButton(text: "333")}{MyButton(text: "444")}
            <br />
            <br />

            { names.Select(n => NoCButton(text: n)) }
            
            ...and then...
            
            { names.Where(n => n[0] != 't').Select(n => NoCButton(text: n)) }

            <h2>Click on the buttons</h2>
            <div>
            outer div<br />
                <div>
                    middle div<br />
                    <div>
                        inner div<br />
                        <button>allow propagation</button><br />
                        <button id="stopPropagation">stop propagation</button><br />
                        <button id="stopImmediatePropagation">immediate stop propagation</button>
                    </div>
                </div>
            </div>
            
            {ComponentWithComponentInside()}

            and one more for that big time key-rewind
            {MyButton(text: "Nice to repeat you")}

        </body>
    </html>
    """);

Html MyButton(string text, Action<Event>? onClick = null) => $"""
    <button onclick={onClick ?? OnClick}>
        Hello {text}! Clicks: {c}
    </button>
    """;

Html YourButton(string? text = null) => $"""
    <button>
        Clicks: {c} - {text ?? "whatevs"}
    </button>
    """;

Html NoCButton(string text) => $"""
    
    <span>{text}, </span>
    """;

Html NoneButton() => $"""
    <p>None</p>
    """;

Html ComponentWithComponentInside() => $"""
    <div>
        {MyButton(text: "3rd level?")}
    </div>
""";

static void OnClick(Event.Mouse e)
{
    // c++;

    // Console.WriteLine(e.X);
    var x = e.X;
}

// window.OnClick = e => Console.WriteLine($"win0: window.OnClick 0: {"e"}");
// window.AddEventListener("afterprint", (Event e) => Console.WriteLine($"win1: afterprint: {"e"}"));
// window.AddEventListener("dblclick", () => Console.WriteLine("win2: dblclick:"));
// window.AddEventListener("click", e => d = e.X);
// window.OnClick = e => Console.WriteLine($"win4: window.OnClick 1: {"e"}");
// window.Document.OnSelectionChange = e => Console.WriteLine($"doc5: {e.Type}");
// window.Document.AddEventListener("click", e => Console.WriteLine($"doc6: document.onclick: {e.X}"));
window.AddEventListener("mousemove", e => c = (int)e.X);

window.MapGet("/about", ctx => 
{
    name = "Reset";
    c = 0;
});

app.Run();
