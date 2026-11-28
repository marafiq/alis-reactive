using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules
{
    public const string Identity = "Identity";

    internal const string ByDefaultAndForTheIdentity = "default, " + Identity;

    partial void TheResident() => RuleSet(ByDefaultAndForTheIdentity, () =>
    {
        RuleFor(moveIn => moveIn.ResidentId).NotEmpty().WithName("Resident").WithMessage("Choose a resident.");
        RuleFor(moveIn => moveIn.Resident).SetValidator(new ResidentRules());
        RuleFor(moveIn => moveIn.Resident.Identity.BirthDate).LessThan(moveIn => moveIn.Today).WithMessage("The birth date is before today.").ServerOnly();
        RuleFor(moveIn => moveIn.Resident.MoveInDate).GreaterThanOrEqualTo(moveIn => moveIn.Today).WithMessage("Move-in cannot precede today.").ServerOnly();
    });
}

public sealed class ResidentRules : ReactiveValidator<ResidentInput>
{
    public ResidentRules() => RuleSet(MoveInRules.ByDefaultAndForTheIdentity, () =>
    {
        RuleFor(resident => resident.Identity).NotNull().ChildRules(identity => identity.RuleSet(MoveInRules.ByDefaultAndForTheIdentity, () =>
        {
            identity.RuleFor(held => held.Name).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Enter the resident\u2019s name.").Length(2, 100).WithMessage("A name holds 2 to 100 letters.");
            identity.RuleFor(held => held.PreferredName).MaximumLength(50).WithMessage("Keep the preferred name to 50 letters.").Unless(held => held.PreferredName == null);
            identity.RuleFor(held => held.Email).NotEmpty().WithMessage("Enter a valid resident email.").EmailAddress().WithMessage("Enter a valid resident email.");
            identity.RuleFor(held => held.EmailConfirmation).Equal(held => held.Email).WithMessage("The email addresses must match.");
        }));
        RuleFor(resident => resident.Address).SetValidator(new AddressRules());
        RuleFor(resident => resident.Notes).MaximumLength(500).WithMessage("Keep the notes to 500 letters.");
    });
}

public sealed class AddressRules : ReactiveValidator<Address>
{
    public AddressRules() => RuleSet(MoveInRules.ByDefaultAndForTheIdentity, () =>
    {
        RuleFor(address => address.CountryCode).Length(2).WithMessage("Use a two-letter country code.");
        RuleFor(address => address.PostalCode).Matches("^[0-9]{5}$").WithMessage("A postal code of the US holds five digits.").When(address => address.CountryCode == "US");
    });
}
