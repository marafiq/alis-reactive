using System.Web;
using System.Web.Mvc;
using Alis.Reactive.Conformance.App.World;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Conformance.App;

internal sealed class TwinResolver(IDependencyResolver mvcs, IServiceProvider container) : IDependencyResolver
{
    public object? GetService(Type serviceType)
        => LeftUnset ? mvcs.GetService(serviceType) : container.GetService(serviceType) ?? mvcs.GetService(serviceType);

    public IEnumerable<object> GetServices(Type serviceType)
        => LeftUnset ? mvcs.GetServices(serviceType) : container.GetServices(serviceType).OfType<object>().Concat(mvcs.GetServices(serviceType));

    private static bool LeftUnset => HttpContext.Current is { } context && TwinWorlds.OpenFor(new HttpContextWrapper(context)) is { ResolverLeftUnset: true };
}
