using Web4;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
int c = 0;

app.Map("/", () => $"""
    <html>
        <body>
            <button onclick={() => c++}>
                Clicks: {c}
            </button>
        </body>
    </html>
    """);

app.Run();
