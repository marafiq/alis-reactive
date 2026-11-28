using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.TypeKnowledge;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Validation;

internal sealed class ValidatorRegistration(Type validatorType, Func<IServiceProvider, ReactiveValidator> resolve)
{
    internal Type ValidatorType { get; } = validatorType;
    internal ReactiveValidator Resolve(IServiceProvider services) => resolve(services);
}

internal sealed class ValidatorCatalog
{
    private readonly Lazy<IReadOnlyDictionary<Type, ValidatorRegistration>> _models;

    internal ValidatorCatalog(IServiceScopeFactory scopes, IEnumerable<ValidatorRegistration> registrations)
    {
        var declared = registrations.ToArray();
        _models = new Lazy<IReadOnlyDictionary<Type, ValidatorRegistration>>(() => Discover(scopes, declared));
    }

    internal bool Checks(Type model) => Find(model) is not null;

    internal ValidatorRegistration? Find(Type model)
        => _models.Value.TryGetValue(model, out var registration) ? registration : null;

    internal TValidator Resolve<TRequest, TValidator>(IServiceProvider services) where TValidator : ReactiveValidator<TRequest>
    {
        var registration = Find(typeof(TRequest));
        if (registration is null || registration.ValidatorType != typeof(TValidator))
            throw Missing<TRequest, TValidator>();
        return (TValidator)registration.Resolve(services);
    }

    internal static InvalidOperationException Missing<TRequest, TValidator>()
        => new($"{ClassNames.AsCSharpWritesIt(typeof(TRequest))} has no registered {ClassNames.AsCSharpWritesIt(typeof(TValidator))} validator. Register it with services.AddReactiveFluentValidation(validation => validation.Add<{ClassNames.AsCSharpWritesIt(typeof(TValidator))}>()).");

    private static IReadOnlyDictionary<Type, ValidatorRegistration> Discover(IServiceScopeFactory scopes, IEnumerable<ValidatorRegistration> registrations)
    {
        using var scope = scopes.CreateScope();
        var models = new Dictionary<Type, ValidatorRegistration>();
        foreach (var registration in registrations)
        {
            var model = registration.Resolve(scope.ServiceProvider).ModelType;
            if (models.TryGetValue(model, out var previous))
                throw new InvalidOperationException($"{ClassNames.AsCSharpWritesIt(model)} has more than one registered validator: {ClassNames.AsCSharpWritesIt(previous.ValidatorType)} and {ClassNames.AsCSharpWritesIt(registration.ValidatorType)}. Register one validator for {ClassNames.AsCSharpWritesIt(model)} with Add<TValidator>(); put its alternate checks in rule sets.");
            models.Add(model, registration);
        }
        return models;
    }
}
