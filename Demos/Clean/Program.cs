using Xui.Web;
using Xui.Web.HttpX;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var c = 4;

var window = app.MapXttp("/app", () => $"""
    <html>
        <body>
            <button onclick={OnClick}>
                Clicks: {c}
            </button>
        </body>
    </html>
    """);

void OnClick(Event e)
{
    c++;
    Console.WriteLine($"{e}");
}

window.AddEventListener("scroll", e =>
{
    Console.WriteLine($"{e}");
});

