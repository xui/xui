using Xui.Web;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string name = "World";
int c = 0;

var window = app.Map("/app", () => $"""
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

window.AddEventListener("click", e => {
    Console.WriteLine($"{e}");
});

window.MapGet("/about", ctx => {
    name = "Reset";
    c = 0;
});

app.Run();
