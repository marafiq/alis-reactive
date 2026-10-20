using Alis.Reactive.FluentValidator;
using FeatureFlags.Domain;
using FeatureFlags.Models;
using FluentValidation;

namespace FeatureFlags.Validators;

/// <summary>Saving an existing flag: the name is the only free-text field left to check.</summary>
public class FlagEditValidator : ReactiveValidator<FlagEditModel>
{
    public FlagEditValidator()
    {
        ClientRule(x => x.Name)
            .Required("Give the flag a name.")
            .MaxLength(80, "Keep the name under 80 characters.");
    }
}

/// <summary>
/// Creating a flag: name and key shape are checked in the browser; whether the key is
/// already used in the app is a server-only rule.
/// </summary>
public class NewFlagValidator : ReactiveValidator<FlagEditModel>
{
    public NewFlagValidator(FlagStore store)
    {
        ClientRule(x => x.Name)
            .Required("Give the flag a name.")
            .MaxLength(80, "Keep the name under 80 characters.");

        ClientRule(x => x.Key)
            .Required("Add a key.")
            .Regex(FlagKeys.Pattern, FlagKeys.PatternMessage);

        RuleFor(x => x.Key)
            .Must((form, key) => store.KeyProblem(form.App, key ?? "", exceptId: null) is null)
            .When(form => !string.IsNullOrWhiteSpace(form.Key) && FlagKeys.HasValidShape(form.Key.Trim()))
            .WithMessage(form => store.KeyProblem(form.App, form.Key ?? "", exceptId: null) ?? "");
    }
}
