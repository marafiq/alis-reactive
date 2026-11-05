namespace Alis.Reactive.Framework.App.Validation;

public sealed class DischargeSummary
{
    public string? Resident { get; set; } = "Ada";

    public string? Ward { get; set; } = "East";

    public string? Summary { get; set; } = "Discharged home with family support.";

    public string? SignedBy { get; set; } = "Dr Bell";

    public Transport Transport { get; set; } = new();

    public string? Medication { get; set; } = "None";
}

public sealed class Transport
{
    public string? Carrier { get; set; } = "Family car";
}

public sealed class DischargeRules : ReactiveValidator<DischargeSummary>
{
    public DischargeRules()
    {
        RuleFor(x => x.Ward).NotEmpty().WithMessage("Name the ward.");
        RuleSet("Draft, SignOff", () =>
        {
            RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
            RuleFor(x => x.Transport).SetValidator(new TransportRules());
        });
        RuleSet("Draft", () => RuleFor(x => x.Summary).NotEmpty().WithMessage("Write the summary before saving a draft."));
        RuleSet("SignOff", () =>
        {
            RuleFor(x => x.Summary).MinimumLength(20).WithMessage("A signed-off summary says more.");
            RuleFor(x => x.SignedBy).Must(name => name is "Dr Bell" or "Dr Ng").WithMessage("Only a doctor on the ward signs off.");
        });
        Include(new HandbackRules());
    }
}

public sealed class TransportRules : ReactiveValidator<Transport>
{
    public TransportRules()
    {
        RuleSet("SignOff", () => RuleFor(x => x.Carrier).NotEmpty().WithMessage("Name who takes the resident home."));
    }
}

public sealed class HandbackRules : ReactiveValidator<DischargeSummary>
{
    public HandbackRules()
    {
        RuleSet("Draft", () => RuleFor(x => x.Medication).NotEmpty().WithMessage("List the medication to hand back."));
    }
}
