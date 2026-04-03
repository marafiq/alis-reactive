using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class RuleExtractorFactory
{
    internal static FluentValidationRuleExtractor Create() =>
        new(type => (IValidator?)Activator.CreateInstance(type));
}
