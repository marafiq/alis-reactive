using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.PlanTests.Support;

internal static class Registered
{
    internal static Action<IServiceCollection> Validator<TValidator>() where TValidator : ReactiveValidator
        => services => services.AddReactiveFluentValidation(validation => validation.Add<TValidator>());

    internal static Action<IServiceCollection> Nothing() => _ => { };
}
