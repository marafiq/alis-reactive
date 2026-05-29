using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Alis.Reactive.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alis.Reactive.FluentValidator
{
    public static class ReactiveFluentValidationServiceCollectionExtensions
    {
        public static IServiceCollection AddReactiveFluentValidation(
            this IServiceCollection services,
            Action<ReactiveFluentValidationBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            services.TryAddSingleton<IClientValidationRuleSource, ClientValidationRuleSource>();

            var builder = new ReactiveFluentValidationBuilder(services);
            configure(builder);
            var clientMetadataTypes = builder.ClientMetadataTypes.ToArray();
            if (clientMetadataTypes.Length != 0)
            {
                services.AddSingleton<IClientValidationMetadataProvider>(provider =>
                    new ReactiveValidatorClientMetadataProvider(
                        provider.GetRequiredService<IServiceScopeFactory>(),
                        clientMetadataTypes));
            }

            return services;
        }
    }

    public sealed class ReactiveFluentValidationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly List<Type> _clientMetadataTypes = new List<Type>();

        internal ReactiveFluentValidationBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        internal IReadOnlyList<Type> ClientMetadataTypes => _clientMetadataTypes;

        public ReactiveFluentValidationBuilder Add<TValidator>()
            where TValidator : class, IValidator
        {
            Add(typeof(TValidator));
            return this;
        }

        public ReactiveFluentValidationBuilder AddFromAssemblyContaining<TMarker>() =>
            AddFromAssembly(typeof(TMarker).Assembly);

        public ReactiveFluentValidationBuilder AddFromAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var validatorType in ValidatorTypesIn(assembly))
            {
                AddValidatorServices(validatorType);
                if (DeclaresClientMetadata(validatorType))
                    AddClientMetadataSource(validatorType);
            }

            return this;
        }

        private void Add(Type validatorType)
        {
            AddValidatorServices(validatorType);
            if (DeclaresClientMetadata(validatorType))
                AddClientMetadataSource(validatorType);
        }

        private static IEnumerable<Type> ValidatorTypesIn(Assembly assembly) =>
            assembly.GetTypes()
                .Where(type => !type.IsAbstract)
                .Where(type => !type.IsGenericTypeDefinition)
                .Where(type => typeof(IValidator).IsAssignableFrom(type));

        private void AddClientMetadataSource(Type validatorType)
        {
            if (!_clientMetadataTypes.Contains(validatorType))
                _clientMetadataTypes.Add(validatorType);
        }

        private void AddValidatorServices(Type validatorType)
        {
            _services.AddTransient(validatorType);

            foreach (var serviceType in ValidatorServiceTypes(validatorType))
                _services.AddTransient(serviceType, validatorType);
        }

        private static bool DeclaresClientMetadata(Type validatorType) =>
            typeof(IClientValidationMetadataSource).IsAssignableFrom(validatorType);

        private static IEnumerable<Type> ValidatorServiceTypes(Type validatorType) =>
            validatorType.GetInterfaces()
                .Where(type => type.IsGenericType)
                .Where(type => type.GetGenericTypeDefinition() == typeof(IValidator<>));
    }

    internal sealed class ReactiveValidatorClientMetadataProvider : IClientValidationMetadataProvider
    {
        private readonly FrozenDictionary<Type, IReadOnlyList<ClientValidationField>> _clientRules;

        public ReactiveValidatorClientMetadataProvider(
            IServiceScopeFactory scopeFactory,
            IEnumerable<Type> validationSourceTypes)
        {
            if (scopeFactory == null) throw new ArgumentNullException(nameof(scopeFactory));
            if (validationSourceTypes == null) throw new ArgumentNullException(nameof(validationSourceTypes));

            _clientRules = BuildClientRulesBySource(
                scopeFactory,
                validationSourceTypes);
        }

        public bool TryGetClientRules(
            Type validationSourceType,
            [NotNullWhen(true)]
            out IReadOnlyList<ClientValidationField>? fields)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));
            return _clientRules.TryGetValue(validationSourceType, out fields);
        }

        private static FrozenDictionary<Type, IReadOnlyList<ClientValidationField>> BuildClientRulesBySource(
            IServiceScopeFactory scopeFactory,
            IEnumerable<Type> validationSourceTypes)
        {
            using var scope = scopeFactory.CreateScope();
            var rulesBySource = new Dictionary<Type, IReadOnlyList<ClientValidationField>>();
            foreach (var validationSourceType in validationSourceTypes.Distinct())
            {
                var metadata = (IClientValidationMetadataSource)scope.ServiceProvider.GetRequiredService(validationSourceType);
                rulesBySource.Add(validationSourceType, metadata.GetClientRules());
            }

            return rulesBySource.ToFrozenDictionary();
        }
    }
}
