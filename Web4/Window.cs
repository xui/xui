using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Web4;

public struct Context
{
    public Window Window { get; }
    public HttpRequest Request { get; }
    public HttpResponse Response { get; }
    public ConnectionInfo Connection { get; }
    public ISession Session { get; }
    public ClaimsPrincipal User { get; set; }
}

public class Window
{
}