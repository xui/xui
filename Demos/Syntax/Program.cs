global using Web4;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddXui();

var app = builder.Build();
app.MapUI("/", new UI());

app.Run();