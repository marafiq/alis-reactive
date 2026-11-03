namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A discharge summary, saved as a draft or signed off: each stage has rules of its own.</summary>
public sealed class DischargeSummary
{
    public string? Resident { get; set; } = "Ada";

    public string? Ward { get; set; } = "East";

    public string? Summary { get; set; } = "Discharged home with family support.";

    public string? SignedBy { get; set; } = "Dr Bell";

    public Transport Transport { get; set; } = new();

    public string? Medication { get; set; } = "None";
}

/// <summary>How the resident leaves.</summary>
public sealed class Transport
{
    public string? Carrier { get; set; } = "Family car";
}

/// <summary>
/// The rules of a discharge by stage: the resident and the transport for every stage, the summary's presence
/// for a draft, its length and the signature for the sign-off, the ward for no stage, and the handback's
/// rules brought in whatever the stage.
/// </summary>
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

/// <summary>The transport's rule, asked at the sign-off alone.</summary>
public sealed class TransportRules : ReactiveValidator<Transport>
{
    public TransportRules()
    {
        RuleSet("SignOff", () => RuleFor(x => x.Carrier).NotEmpty().WithMessage("Name who takes the resident home."));
    }
}

/// <summary>The handback's rule, asked of a draft alone.</summary>
public sealed class HandbackRules : ReactiveValidator<DischargeSummary>
{
    public HandbackRules()
    {
        RuleSet("Draft", () => RuleFor(x => x.Medication).NotEmpty().WithMessage("List the medication to hand back."));
    }
}
