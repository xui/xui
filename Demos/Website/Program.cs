global using Web4;
global using Web4.ZeroScript;
global using Web4.HttpX;
global using System.Buffers;
using Web4.Composers;
using Web4.HttpX.Composers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXui();
#if !DEBUG
builder.Services.AddLogging(c => c.ClearProviders()); // TODO: Remove
#endif

var app = builder.Build();
app.MapUI("/", new UI());

app.Run();