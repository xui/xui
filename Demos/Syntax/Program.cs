global using Web4;
global using Web4.ZeroScript;
global using Web4.HttpX;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXui();

var app = builder.Build();
app.MapUI("/", new UI());

app.Run();