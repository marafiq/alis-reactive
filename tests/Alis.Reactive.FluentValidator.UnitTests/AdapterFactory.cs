using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class AdapterFactory
{
    internal static FluentValidationAdapter Create() =>
        new(type => (IValidator?)Activator.CreateInstance(type));
}
