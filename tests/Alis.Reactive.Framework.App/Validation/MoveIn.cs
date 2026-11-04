namespace Alis.Reactive.Framework.App.Validation;

public sealed class MoveInPacket
{
    public string? Resident { get; set; } = "Ada";

    public List<string?> Keys { get; set; } = new() { "K-1", "K-2" };

    public string? Insurer { get; set; } = "Mutual";
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
