using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Registers explicit browser validation metadata for reactive plans.
    /// </summary>
    public static class ReactiveClientValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddReactiveClientValidation(
            this IServiceCollection services,
            Action<ReactiveClientValidationBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            services.TryAddSingleton<IClientValidationRuleSource, ClientValidationRuleSource>();

            var builder = new ReactiveClientValidationBuilder();
            configure(builder);
            services.AddSingleton<IClientValidationMetadataProvider>(builder.Build());
            return services;
        }
    }

    /// <summary>
    /// Registers app-level client validation metadata keyed by validation source type.
    /// </summary>
    public sealed class ReactiveClientValidationBuilder
    {
        private readonly List<ClientValidationRuleSetDefinition> _ruleSets = new List<ClientValidationRuleSetDefinition>();

        internal ReactiveClientValidationBuilder() { }

        public ReactiveClientValidationBuilder Add<TValidationSource, TModel>(
            Action<ClientValidationRulesBuilder<TModel>> define)
            where TModel : class
        {
            if (define == null) throw new ArgumentNullException(nameof(define));

            _ruleSets.Add(ClientValidationRuleSetDefinition.For<TValidationSource, TModel>(define));
            return this;
        }

        internal IClientValidationMetadataProvider Build() =>
            new ConfiguredClientValidationMetadataProvider(_ruleSets);
    }
}
