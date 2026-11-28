#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The startup call that configures the app's server push.
/// </summary>
public static class RealtimeServices
{
    /// <summary>Configures what every stream the app's views name takes, once at startup: the reconnect choice and the reconnection interval.</summary>
    /// <remarks>The reconnect choice is required: a view that names a stream in an app without it is refused, naming <c>ShowReconnect()</c> and <c>NoReconnectOffer()</c>.</remarks>
    /// <param name="services">The app's services.</param>
    /// <param name="live">The choices.</param>
    /// <returns>The app's services.</returns>
    public static IServiceCollection AddReactiveRealtime(this IServiceCollection services, Action<RealtimeChoices> live)
    {
        services.TryAddSingleton(RealtimeChoices.Configured(live));
        return services;
    }
}
#endif
