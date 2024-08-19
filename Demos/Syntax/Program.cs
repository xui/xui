global using Xui.Web;
global using Xui.Web.ZeroScript;
global using Xui.Web.HttpX;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXui();

var app = builder.Build();
app.MapUI("/", new UI());

app.Run();