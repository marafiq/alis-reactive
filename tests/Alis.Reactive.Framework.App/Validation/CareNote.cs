namespace Alis.Reactive.Framework.App.Validation;

public sealed class CareNote
{
    public string? Observation { get; set; }

    public string? Reviewer { get; set; } = "yes";

    public string? Initials { get; set; } = "yes";

    public string? Witness { get; set; } = "yes";
}

public sealed class CareNoteRules : ReactiveValidator<CareNote>
{
    public CareNoteRules()
    {
        RuleFor(x => x.Observation).NotEmpty().WithMessage("Write what you observed.");
        RuleFor(x => x.Reviewer).NotEmpty().When(x => x.Observation != null && x.Observation.Length > 20).WithMessage("A long observation needs a reviewer.");
        RuleFor(x => x.Initials).NotEmpty().When(x => x.Observation!.Length < 5).WithMessage("A short observation needs your initials.");
        RuleFor(x => x.Witness).NotEmpty().When(x => x.Observation != null && 40 <= x.Observation.Length).WithMessage("An observation of forty letters or more needs a witness.");
    }
}
