using System;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive
{
    /// <summary>Registers the class names an app's input fields render with.</summary>
    public static class ReactiveClassNamesServiceCollectionExtensions
    {
        /// <summary>
        /// Chooses the class names every input field renders with in place of the framework's: the wrapper, its
        /// label, the required marker, the message slot and the class a refused input carries. A name the app
        /// leaves unset keeps its default. Call it once at startup; the names apply to every page.
        /// </summary>
        /// <example><c>services.AddReactiveClassNames(names =&gt; names.InvalidInput = "is-invalid");</c></example>
        public static IServiceCollection AddReactiveClassNames(
            this IServiceCollection services,
            Action<ReactiveClassNames> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var names = new ReactiveClassNames();
            configure(names);
            services.AddSingleton(names);
            return services;
        }
    }
}
