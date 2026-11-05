
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class IntakeStepsModel
    {
        public string? ResidentName { get; set; }
        public string? FamilyPhone { get; set; }
        public string? CareLevel { get; set; }

        public string? MobilityAid { get; set; }
        public string? MobilityNotes { get; set; }
    }

    public class IntakeResidentValidator : ReactiveValidator<IntakeStepsModel>
    {
        public IntakeResidentValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleFor(x => x.FamilyPhone)
                .NotEmpty().WithMessage("Enter a family phone number.")
                .Matches(@"^\d{3}-\d{4}$").WithMessage("Enter the family phone as 555-0100.");
        }
    }

    public class IntakeCareValidator : ReactiveValidator<IntakeStepsModel>
    {
        public const string FullCareLevel = "Memory care";

        public IntakeCareValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Choose a care level.");
            RuleFor(x => x.CareLevel)
                .Must(level => level != FullCareLevel)
                .WithMessage($"{FullCareLevel} has no open room this month. Choose another level or add the resident to the waitlist.");
            RuleFor(x => x.MobilityAid).NotEmpty().WithMessage("Choose the resident's mobility aid.");
            RuleFor(x => x.MobilityNotes).MaximumLength(40).WithMessage("Keep mobility notes under 40 characters.");
        }
    }

    public sealed record IntakeConfirmation(string Confirmation);
}
