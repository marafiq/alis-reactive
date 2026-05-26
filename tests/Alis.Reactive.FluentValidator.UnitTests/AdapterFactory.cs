using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class AdapterFactory
{
    internal static FluentValidationAdapter Create() =>
        new(type => (IValidator?)Activator.CreateInstance(type));

    internal static IReadOnlyList<ClientValidationField> ProjectRules(
        this FluentValidationAdapter adapter,
        Type validatorType,
        string validationContainerId) =>
        adapter.ProjectClientRules(validatorType);

    internal static IReadOnlyList<ClientValidationField> ProjectFields(
        this FluentValidationAdapter adapter,
        Type validatorType,
        string validationContainerId) =>
        adapter.ProjectClientRules(validatorType);
}
