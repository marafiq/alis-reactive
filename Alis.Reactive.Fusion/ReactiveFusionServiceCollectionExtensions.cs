using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alis.Reactive.Fusion
{
    /// <summary>
    /// Sets up an application to render Fusion (Syncfusion) components.
    /// </summary>
    public static class ReactiveFusionServiceCollectionExtensions
    {
        // Camel cases every name that is not pinned, dictionary keys included, and keeps the names
        // pinned with [JsonProperty]. One shared instance, so Newtonsoft's contract cache is reused.
        private static readonly IContractResolver FusionNaming = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = false
            }
        };

        /// <summary>
        /// Sets up the application to render Fusion components: your data sources reach the browser
        /// in camel case, and Syncfusion's own options keep the names Syncfusion expects.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Syncfusion writes every component's options, and the data sources you give it, with
        /// Newtonsoft.Json's process-wide <c>JsonConvert.DefaultSettings</c>. Field mappings such as
        /// <c>Fields&lt;TItem&gt;(...)</c> name your item's members in camel case, so the data has to
        /// be camel case too. A few Syncfusion options pin a different name with
        /// <c>[JsonProperty]</c>: a toast's and a dialog's position use <c>"X"</c> and <c>"Y"</c>.
        /// A plain camel-case setting renames those as well, and the toast falls back to the top left.
        /// </para>
        /// <para>
        /// This method sets that naming on <c>JsonConvert.DefaultSettings</c> and keeps everything
        /// else your settings already carry, such as converters, date handling and null handling.
        /// Call it after any line that assigns <c>JsonConvert.DefaultSettings</c>: a later
        /// assignment replaces it. Calling it more than once has no further effect.
        /// </para>
        /// </remarks>
        /// <param name="services">The application's service collection.</param>
        /// <returns>The same service collection, for chaining.</returns>
        /// <example>
        /// <code>
        /// builder.Services.AddReactiveFusion();
        /// </code>
        /// </example>
        public static IServiceCollection AddReactiveFusion(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            UseFusionNamingForNewtonsoftDefaults();
            return services;
        }

        private static void UseFusionNamingForNewtonsoftDefaults()
        {
            // Newtonsoft leaves DefaultSettings null until an app assigns it; null means its plain
            // defaults, which is what a new JsonSerializerSettings carries.
            var appSettings = JsonConvert.DefaultSettings ?? (() => new JsonSerializerSettings());
            var alreadyUsesFusionNaming = appSettings().ContractResolver == FusionNaming;
            if (alreadyUsesFusionNaming) return;

            JsonConvert.DefaultSettings = () => WithFusionNaming(appSettings());
        }

        private static JsonSerializerSettings WithFusionNaming(JsonSerializerSettings settings)
        {
            settings.ContractResolver = FusionNaming;
            return settings;
        }
    }
}
