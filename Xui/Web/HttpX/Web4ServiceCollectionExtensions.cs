#pragma warning disable IDE0130 // Namespace does not match folder structure

using Xui.Web.Composers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xui.Web;
using Xui.Web.HttpX;
using Microsoft.AspNetCore.WebSockets;

namespace Microsoft.Extensions.DependencyInjection;

public static class Web4ServiceCollectionExtensions
{
    public static IServiceCollection AddWeb4(this IServiceCollection services)
    {
        services.AddWebSockets(options => {});
        return services;
    }
}