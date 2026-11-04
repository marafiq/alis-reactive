namespace Alis.Reactive.Framework.App.Validation;

public sealed class FallReport
{
    public string? Resident { get; set; }

    public string? Location { get; set; }

    public string? Injury { get; set; }
}

public sealed class FallReportRules : ReactiveValidator<FallReport>
{
    public FallReportRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident who fell.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Say where the fall happened.");
        RuleSet("Nurse", () => RuleFor(x => x.Injury).NotEmpty().WithMessage("Describe the injury for the nurse."));
    }
}
