using System;
using Alis.Reactive.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The startup call that chooses what the app's traced pages do.
/// </summary>
public static class TracingServices
{
    /// <summary>Chooses what every traced page of the app does, once at startup.</summary>
    /// <remarks>
    /// An app that never calls it traces with every default:
    /// <c>services.AddReactiveTracing(tracing =&gt; tracing.Console = builder.Environment.IsDevelopment());</c>. On MVC 5 a
    /// view reads the choice through MVC's resolver, set over the app's container with <c>DependencyResolver.SetResolver</c>.
    /// </remarks>
    /// <param name="services">The app's services.</param>
    /// <param name="tracing">Sets what the app chooses.</param>
    /// <returns>The app's services.</returns>
    public static IServiceCollection AddReactiveTracing(this IServiceCollection services, Action<ReactiveTracing> tracing)
    {
        var chosen = new ReactiveTracing();
        tracing(chosen);
        return services.AddSingleton(chosen);
    }
}
