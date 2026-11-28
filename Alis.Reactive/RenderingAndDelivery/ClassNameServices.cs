using System;
using Alis.Reactive.InputsAndIds;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// The startup call that chooses the class names the app's input fields render with.
/// </summary>
public static class ClassNameServices
{
    /// <summary>Chooses the class names every input field of the app renders with, once at startup.</summary>
    /// <remarks>
    /// Each name the app leaves unset keeps its default, and an app that never calls it renders every default:
    /// <c>services.AddReactiveClassNames(names =&gt; names.InvalidInput = "is-invalid");</c>. On MVC 5 a view reads
    /// the names through MVC's resolver, set over the app's container with <c>DependencyResolver.SetResolver</c>.
    /// </remarks>
    /// <param name="services">The app's services.</param>
    /// <param name="names">Sets the names the app chooses.</param>
    /// <returns>The app's services.</returns>
    public static IServiceCollection AddReactiveClassNames(this IServiceCollection services, Action<ReactiveClassNames> names)
    {
        var chosen = new ReactiveClassNames();
        names(chosen);
        return services.AddSingleton(chosen);
    }
}
