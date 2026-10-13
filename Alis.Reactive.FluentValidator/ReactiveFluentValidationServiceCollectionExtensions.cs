using System;
using System.Collections.Generic;
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

            // Transient: the browser's rules are read from the validator the request resolves, so a
            // scoped dependency, DateTime.Today or a captured setting is the one this request's server sees.
            services.TryAddTransient<IClientValidationRuleSource, ReactiveValidatorRuleSource>();

            var builder = new ReactiveFluentValidationBuilder(services);
            configure(builder);
            services.AddSingleton(new ReactiveValidatorTypes(builder.ClientMetadataTypes.ToArray()));
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

    // The validators one AddReactiveFluentValidation call registered as sources of browser rules.
    internal sealed class ReactiveValidatorTypes
    {
        internal ReactiveValidatorTypes(IReadOnlyList<Type> types)
        {
            Types = types;
        }

        internal IReadOnlyList<Type> Types { get; }
    }

    // The browser rules of the validator this request resolves, read when the page renders.
    internal sealed class ReactiveValidatorRuleSource : IClientValidationRuleSource
    {
        private readonly IServiceProvider _services;
        private readonly HashSet<Type> _validators;

        public ReactiveValidatorRuleSource(IServiceProvider services, IEnumerable<ReactiveValidatorTypes> registered)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _validators = new HashSet<Type>(registered.SelectMany(types => types.Types));
        }

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));
            if (!_validators.Contains(validationSourceType))
                throw new InvalidOperationException(
                    $"No Reactive Plan client validation metadata is registered for validation source '{validationSourceType.FullName}'. " +
                    "Register its validator with services.AddReactiveFluentValidation(rules => rules.Add<TValidator>()).");

            var validator = (IClientValidationMetadataSource)_services.GetRequiredService(validationSourceType);
            return validator.GetClientRules();
        }
    }
}
