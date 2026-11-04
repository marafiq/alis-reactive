namespace Alis.Reactive.Framework.App.Validation;

public sealed class MoveInPacket
{
    public string? Resident { get; set; } = "Ada";

    public List<string?> Keys { get; set; } = new() { "K-1", "K-2" };

    public string? Insurer { get; set; } = "Mutual";

    public MoveInPharmacy Pharmacy { get; set; } = new();
}

public sealed class MoveInPharmacy
{
    public string? Phone { get; set; } = "555-0101";
}

public sealed class MoveInRules : ReactiveValidator<MoveInPacket>
{
    public MoveInRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
        RuleSet("Arrive", () =>
        {
            RuleForEach(x => x.Keys).NotEmpty().WithMessage("Log each key handed over.");
            Include(new MoveInBillingRules());
            RuleFor(x => x.Pharmacy).ChildRules(pharmacy => pharmacy.RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Give the pharmacy's phone.")
                .Must(phone => phone != null && phone.StartsWith("555-")).WithMessage("Use a 555 number."));
        });
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
