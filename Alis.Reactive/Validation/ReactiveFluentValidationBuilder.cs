using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alis.Reactive.Validation;

/// <summary>
/// The validators one <c>AddReactiveFluentValidation</c> call registers.
/// </summary>
public sealed class ReactiveFluentValidationBuilder
{
    private readonly IServiceCollection _services;

    internal ReactiveFluentValidationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>Registers one validator, named by its type.</summary>
    /// <remarks>
    /// Each validator is registered by a call of its own: no assembly is scanned for validators. An existing service
    /// registration is kept; otherwise the validator is scoped. Model discovery constructs it in a disposable scope,
    /// so its constructor must work outside an HTTP request.
    /// </remarks>
    /// <typeparam name="TValidator">The validator.</typeparam>
    /// <returns>The builder.</returns>
    public ReactiveFluentValidationBuilder Add<TValidator>()
        where TValidator : ReactiveValidator
    {
        _services.TryAddScoped<TValidator>();
        _services.AddSingleton(new ValidatorRegistration(typeof(TValidator), services => services.GetRequiredService<TValidator>()));
        return this;
    }
}
