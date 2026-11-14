using System;

namespace Alis.Reactive.Validation;

/// <summary>
/// The validators and the flags enums one <c>AddReactiveFluentValidation</c> call registers.
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

    /// <summary>Names an enum the app reads as flags, once, so a value that combines its members is sent and checked as their names.</summary>
    /// <remarks>An enum it does not name is one value: <c>IsInEnum</c> reads it as <c>Enum.IsDefined</c> does, and a combination a source holds stops its request. Naming one twice names nothing new.</remarks>
    /// <typeparam name="TEnum">The flags enum.</typeparam>
    /// <returns>The builder.</returns>
    public ReactiveFluentValidationBuilder Flags<TEnum>()
        where TEnum : struct, Enum
        => throw new NotImplementedException();
}
