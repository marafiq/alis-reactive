using System;
using Alis.Reactive.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The startup call that registers the app's validators.
/// </summary>
public static class ValidationServices
{
    /// <summary>Registers the app's validators, once at startup.</summary>
    /// <remarks>
    /// A request whose model has a registered validator is checked by it under the default rule set, unless the
    /// request names another rule set or no check.
    /// </remarks>
    /// <param name="services">The app's services.</param>
    /// <param name="validators">Registers each validator.</param>
    /// <returns>The app's services.</returns>
    public static IServiceCollection AddReactiveFluentValidation(this IServiceCollection services, Action<ReactiveFluentValidationBuilder> validators)
        => throw new NotImplementedException();
}
