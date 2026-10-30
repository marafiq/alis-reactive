using Alis.Reactive.FluentValidator;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.PlanTests.Support;

/// <summary>The services a page renders with, as an app registers them.</summary>
internal static class Registered
{
    internal static IServiceProvider Validator<TValidator>() where TValidator : ReactiveValidator =>
        new ServiceCollection().AddReactiveFluentValidation(validation => validation.Add<TValidator>()).BuildServiceProvider();

    internal static IServiceProvider Nothing() => new ServiceCollection().BuildServiceProvider();
}
