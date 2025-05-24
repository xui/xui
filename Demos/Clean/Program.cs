using Web4;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string name = "World";
int c = 0;

var window = app.MapWeb4("/app", () => $"""
    <html>
        <body>
            <button onclick={() => c++}>
                Clicks: {c}
            </button>

            <h1>Hello {name}!</h1>
            {MyButton(text: "I'm a custom button!")}
            {MyButton(text: "Nice to repeat you.", onClick: e => c--)}
        </body>
    </html>
    """);

Html MyButton(string text, Action<Event>? onClick = null) => $"""
    <button onclick={onClick ?? OnClick}>
        {text} Clicks: {c}
    </button>
    """;

void OnClick(Event e)
{
    c++;
}

window.OnClick = e => Console.WriteLine($"win0: window.OnClick 0: {"e"}");
window.AddEventListener("afterprint", (Event e) => Console.WriteLine($"win1: afterprint: {"e"}"));
window.AddEventListener("dblclick", () => Console.WriteLine("win2: dblclick:"));
window.AddEventListener("click", e => Console.WriteLine($"win3: click: {e.X} {e.Y}"));
window.OnClick = e => Console.WriteLine($"win4: window.OnClick 1: {"e"}");

window.Document.OnSelectionChange = e => Console.WriteLine($"doc5: {e.Type}");
window.Document.AddEventListener("click", e => Console.WriteLine($"doc6: document.onclick: {e.X}"));

window.MapGet("/about", ctx => 
{
    name = "Reset";
    c = 0;
});

app.Run();
