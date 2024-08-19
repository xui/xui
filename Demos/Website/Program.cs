global using Xui.Web;
global using Xui.Web.ZeroScript;
global using Xui.Web.HttpX;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXui();
#if !DEBUG
builder.Services.AddLogging(c => c.ClearProviders()); // TODO: Remove
#endif

var app = builder.Build();
app.MapUI("/", new UI());

app.Run();