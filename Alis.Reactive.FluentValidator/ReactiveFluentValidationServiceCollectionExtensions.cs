using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Alis.Reactive.Validation;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddSingleton<IClientValidationRuleSource, FluentValidationClientRuleSource>();
            configure(new ReactiveFluentValidationBuilder(services));
            return services;
        }
    }

    public sealed class ReactiveFluentValidationBuilder
    {
        private readonly IServiceCollection _services;

        internal ReactiveFluentValidationBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
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

    public sealed class FluentValidationClientRuleSource : IClientValidationRuleSource
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<Type, Lazy<IReadOnlyList<ClientValidationField>>> _clientRules =
            new ConcurrentDictionary<Type, Lazy<IReadOnlyList<ClientValidationField>>>();

        public FluentValidationClientRuleSource(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public IReadOnlyList<ClientValidationField> GetClientRules(Type validationSourceType)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));

            return _clientRules
                .GetOrAdd(validationSourceType, type => new Lazy<IReadOnlyList<ClientValidationField>>(
                    () => BuildClientRules(type),
                    isThreadSafe: true))
                .Value;
        }

        private IReadOnlyList<ClientValidationField> BuildClientRules(Type validationSourceType)
        {
            using var scope = _scopeFactory.CreateScope();
            var validator = scope.ServiceProvider.GetService(validationSourceType) as IValidator;
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
                "Derive it from ReactiveValidator<TModel> and use ClientRule(...) for rules that should run in the browser. " +
                "Use RuleFor, Must, When, and async rules for server-only validation.");
        }
    }
}
