using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class ContactRules
{
    partial void TheAuthority()
        => When(contact => contact.Role != AuthorityRole.Ordinary, () =>
        {
            RuleFor(contact => contact.Authority).NotNull().WithMessage("Enter this contact's authority.");
            RuleFor(contact => contact.Authority!).SetValidator(new AuthorityRules(_isRevoked));
            RuleFor(contact => contact.Authority!).SetValidator(new AuthorityConsentRules()).Unless(contact => contact.Role == AuthorityRole.Guardian);
        }).Otherwise(() => RuleFor(contact => contact.Authority).Empty().WithMessage("An ordinary contact holds no authority."));
}

public sealed class AuthorityRules : ReactiveValidator<AuthorityInput>
{
    public AuthorityRules(Func<string, bool> isRevoked)
    {
        RuleFor(authority => authority.Reference).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter the authority's reference.")
            .MinimumLength(5).WithMessage("An authority's reference has at least five characters.");
        RuleFor(authority => authority.Reference).Must(reference => !isRevoked(reference)).WithMessage("This authority reference was revoked.");
        RuleFor(authority => authority.EndsOn).GreaterThanOrEqualTo(authority => authority.StartsOn).WithMessage("An authority cannot end before it starts.");
    }
}

public sealed class AuthorityConsentRules : ReactiveValidator<AuthorityInput>
{
    public AuthorityConsentRules() => RuleFor(authority => authority.Consent).Equal(true).WithMessage("Authority consent is required");
}
