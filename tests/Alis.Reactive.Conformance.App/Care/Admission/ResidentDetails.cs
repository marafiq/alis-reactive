using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NewResidentDetails
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? EmailConfirmation { get; set; }

    public string? Bed { get; set; }
}

public sealed class ResidentDetailsRules : ReactiveValidator<NewResidentDetails>
{
    public const string Identity = nameof(Identity);

    public ResidentDetailsRules()
    {
        RuleSet("default, " + Identity, () =>
        {
            RuleFor(details => details.Name).NotEmpty().WithMessage("Enter the name.");
            RuleFor(details => details.EmailConfirmation).Equal(details => details.Email).WithMessage("The email addresses must match.");
        });
        RuleFor(details => details.Bed).NotEmpty().WithMessage("Choose a bed.");
    }
}
