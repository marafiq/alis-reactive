using System;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The startup call that configures the app's requests.
/// </summary>
public static class RequestServices
{
    /// <summary>Configures what every request of the app takes, once at startup: its antiforgery choice and its defaults.</summary>
    /// <remarks>The call is required: a view that renders a request in an app without it is refused, naming this call.</remarks>
    /// <param name="services">The app's services.</param>
    /// <param name="requests">The requests' defaults.</param>
    /// <returns>The app's services.</returns>
    public static IServiceCollection AddReactiveRequests(this IServiceCollection services, Action<RequestDefaults> requests)
        => throw new NotImplementedException();
}
