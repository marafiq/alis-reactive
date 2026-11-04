namespace Alis.Reactive.Framework.App.Validation;

public sealed class MoveInPacket
{
    public string? Resident { get; set; } = "Ada";

    public List<string?> Keys { get; set; } = new() { "K-1", "K-2" };

    public string? Insurer { get; set; } = "Mutual";

    public MoveInPharmacy Pharmacy { get; set; } = new();

    public List<MoveInGuest> Guests { get; set; } = new() { new MoveInGuest { Name = "Bo", Phone = "555-0111" } };

    public string? Policy { get; set; } = "P-1001";
}

public sealed class MoveInPharmacy
{
    public string? Phone { get; set; } = "555-0101";

    public string? Fax { get; set; } = "555-0199";
}

public sealed class MoveInGuest
{
    public string? Name { get; set; }

    public string? Phone { get; set; }
}

public sealed class MoveInGuestRules : ReactiveValidator<MoveInGuest>
{
    public MoveInGuestRules()
    {
        RuleSet("Arrive", () => RuleFor(x => x.Name).NotEmpty().WithMessage("Name each guest moving in."));
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Give each guest's phone.");
    }
}

public sealed class MoveInPolicyRules : ReactiveValidator<MoveInPacket>
{
    public MoveInPolicyRules()
    {
        RuleFor(x => x.Policy).NotEmpty().WithMessage("Give the policy number.");
        RuleSet("Arrive", () => RuleFor(x => x.Policy).MaximumLength(8).WithMessage("A policy number has eight characters at most."));
    }
}

public sealed class MoveInRules : ReactiveValidator<MoveInPacket>
{
    public MoveInRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
        RuleForEach(x => x.Guests).SetValidator(new MoveInGuestRules());
        Include(new MoveInPolicyRules());
        RuleSet("Arrive", () =>
        {
            RuleForEach(x => x.Keys).NotEmpty().WithMessage("Log each key handed over.");
            Include(new MoveInBillingRules());
            RuleForEach(x => x.Guests).SetValidator(new MoveInGuestRules());
        });
        RuleSet("Arrive, Settle", () => RuleFor(x => x.Pharmacy).ChildRules(pharmacy =>
        {
            pharmacy.RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Give the pharmacy's phone.")
                .Must(phone => phone != null && phone.StartsWith("555-")).WithMessage("Use a 555 number.");
            pharmacy.RuleSet("Settle", () => pharmacy.RuleFor(x => x.Fax).NotEmpty().WithMessage("Give the pharmacy's fax."));
        }));
    }
}

public sealed class MoveInBillingRules : ReactiveValidator<MoveInPacket>
{
    public MoveInBillingRules()
    {
        RuleSet("Arrive", () => RuleFor(x => x.Insurer)
            .NotEmpty().WithMessage("Name the insurer.")
            .Must(insurer => insurer is "Mutual" or "Shield").WithMessage("Bill an insurer the home works with."));
    }
}
