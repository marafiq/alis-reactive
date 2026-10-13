using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Alis.Reactive.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>Registers an app's validators, for its controllers and for the browser rules its pages render.</summary>
    public static class ReactiveFluentValidationServiceCollectionExtensions
    {
        /// <summary>
        /// Registers validators: each as itself and as <see cref="IValidator{T}"/> of its model, and as a source
        /// of the browser rules <c>plan.Validate&lt;TValidator&gt;()</c> renders. A module may call it again
        /// for its own validators; the registrations merge.
        /// </summary>
        /// <example><c>services.AddReactiveFluentValidation(validation =&gt; validation.AddFromAssemblyContaining&lt;Program&gt;());</c></example>
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
            services.AddSingleton(new ReactiveValidatorTypes(builder.Registered.ToArray()));
            return services;
        }
    }

    /// <summary>The validators one <c>AddReactiveFluentValidation</c> call registers.</summary>
    public sealed class ReactiveFluentValidationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly List<Type> _registered = new List<Type>();

        internal ReactiveFluentValidationBuilder(IServiceCollection services)
        {
            _services = services;
        }

        internal IReadOnlyList<Type> Registered => _registered;

        /// <summary>Registers one validator.</summary>
        public ReactiveFluentValidationBuilder Add<TValidator>()
            where TValidator : ReactiveValidator
        {
            Register(typeof(TValidator));
            return this;
        }

        /// <summary>Registers every validator in the assembly that holds <typeparamref name="TMarker"/>.</summary>
        public ReactiveFluentValidationBuilder AddFromAssemblyContaining<TMarker>() =>
            AddFromAssembly(typeof(TMarker).Assembly);

        /// <summary>Registers every validator in the assembly.</summary>
        public ReactiveFluentValidationBuilder AddFromAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var validatorType in assembly.GetTypes().Where(IsValidatorClass))
                Register(validatorType);
            return this;
        }

        private static bool IsValidatorClass(Type type) =>
            !type.IsAbstract && !type.IsGenericTypeDefinition && typeof(ReactiveValidator).IsAssignableFrom(type);

        private void Register(Type validatorType)
        {
            if (_registered.Contains(validatorType)) return;
            _registered.Add(validatorType);

            _services.AddTransient(validatorType);
            foreach (var serviceType in ValidatorServiceTypes(validatorType))
                _services.AddTransient(serviceType, validatorType);
        }

        private static IEnumerable<Type> ValidatorServiceTypes(Type validatorType) =>
            validatorType.GetInterfaces()
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>));
    }

    // The validators one AddReactiveFluentValidation call registered.
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
                    $"No validator '{validationSourceType.FullName}' is registered for the page's browser rules. " +
                    "Register it with services.AddReactiveFluentValidation(validation => validation.Add<TValidator>()).");

            return ((ReactiveValidator)_services.GetRequiredService(validationSourceType)).ClientRules();
        }
    }
}
