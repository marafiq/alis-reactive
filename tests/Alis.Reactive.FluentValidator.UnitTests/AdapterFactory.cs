using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class AdapterFactory
{
    internal static FluentValidationAdapter Create() =>
        new(type => (IValidator?)Activator.CreateInstance(type));

    internal static IReadOnlyList<ValidationField> ExtractRules(
        this FluentValidationAdapter adapter,
        Type validatorType,
        string validationContainerId) =>
        adapter
            .Extract(ValidationExtractionRequest.For(validatorType, validationContainerId))
            .ClientFields;

    internal static ValidationExtractionReport ExtractReport(
        this FluentValidationAdapter adapter,
        Type validatorType,
        string validationContainerId) =>
        adapter.Extract(ValidationExtractionRequest.For(validatorType, validationContainerId));
}
