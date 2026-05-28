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

            var validatorTypes = new HashSet<Type>();
            configure(new ReactiveFluentValidationBuilder(services, validatorTypes));

            var validationSources = validatorTypes.ToArray();
            services.AddSingleton<IClientValidationMetadataProvider>(provider =>
                new FluentValidationClientMetadataProvider(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    validationSources));
            return services;
        }
    }

    public sealed class ReactiveFluentValidationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly ISet<Type> _validatorTypes;

        internal ReactiveFluentValidationBuilder(
            IServiceCollection services,
            ISet<Type> validatorTypes)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _validatorTypes = validatorTypes ?? throw new ArgumentNullException(nameof(validatorTypes));
        }

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
                Add(validatorType);

            return this;
        }

        private void Add(Type validatorType)
        {
            _validatorTypes.Add(validatorType);
            _services.AddTransient(validatorType);

            foreach (var serviceType in ValidatorServiceTypes(validatorType))
                _services.AddTransient(serviceType, validatorType);
        }

        private static IEnumerable<Type> ValidatorTypesIn(Assembly assembly) =>
            assembly.GetTypes()
                .Where(type => !type.IsAbstract)
                .Where(type => !type.IsGenericTypeDefinition)
                .Where(type => typeof(IValidator).IsAssignableFrom(type));

        private static IEnumerable<Type> ValidatorServiceTypes(Type validatorType) =>
            validatorType.GetInterfaces()
                .Where(type => type.IsGenericType)
                .Where(type => type.GetGenericTypeDefinition() == typeof(IValidator<>));
    }

    internal sealed class FluentValidationClientMetadataProvider : IClientValidationMetadataProvider
    {
        private readonly FrozenDictionary<Type, IReadOnlyList<ClientValidationField>> _clientRules;

        public FluentValidationClientMetadataProvider(
            IServiceScopeFactory scopeFactory,
            IEnumerable<Type> validationSourceTypes)
        {
            if (scopeFactory == null) throw new ArgumentNullException(nameof(scopeFactory));
            if (validationSourceTypes == null) throw new ArgumentNullException(nameof(validationSourceTypes));

            _clientRules = BuildClientRulesBySource(scopeFactory, validationSourceTypes);
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
                rulesBySource.Add(validationSourceType, BuildClientRules(scope.ServiceProvider, validationSourceType));

            return rulesBySource.ToFrozenDictionary();
        }

        private static IReadOnlyList<ClientValidationField> BuildClientRules(
            IServiceProvider services,
            Type validationSourceType)
        {
            var validator = services.GetService(validationSourceType) as IValidator;
            if (validator == null)
            {
                throw new InvalidOperationException(
                    $"Validation source '{validationSourceType.FullName}' is not registered as a FluentValidation validator. " +
                    "Register it with services.AddReactiveFluentValidation(rules => rules.Add<TValidator>()) " +
                    "or rules.AddFromAssemblyContaining<TMarker>().");
            }

            if (validator is IClientValidationMetadataSource metadata)
                return metadata.GetClientRules();

            throw new InvalidOperationException(
                $"Validator '{validationSourceType.FullName}' does not declare browser validation metadata. " +
                "Derive it from ReactiveValidator<TModel> and use ClientRule(...) for rules that run in the browser. " +
                "Use RuleFor, Must, When, and async rules for server-only validation.");
        }
    }
}
