using System;

namespace Alis.Reactive.Validation;

/// <summary>
/// The validators one <c>AddReactiveFluentValidation</c> call registers.
/// </summary>
public sealed class ReactiveFluentValidationBuilder
{
    internal ReactiveFluentValidationBuilder()
    {
    }

    /// <summary>Registers one validator, named by its type.</summary>
    /// <remarks>Each validator is registered by a call of its own: no assembly is scanned for validators.</remarks>
    /// <typeparam name="TValidator">The validator.</typeparam>
    /// <returns>The builder.</returns>
    public ReactiveFluentValidationBuilder Add<TValidator>()
        where TValidator : ReactiveValidator
        => throw new NotImplementedException();
}
