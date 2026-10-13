using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A new resident's intake, taken in steps inside one form: the resident first, then the care
    /// they need. Each step's fields arrive with the step and leave when the next one replaces it.
    /// </summary>
    public class IntakeStepsModel
    {
        public string? ResidentName { get; set; }
        public string? FamilyPhone { get; set; }
        public string? CareLevel { get; set; }

        /// <summary>The aid the resident walks with; the care step asks once a care level is chosen.</summary>
        public string? MobilityAid { get; set; }
        public string? MobilityNotes { get; set; }
    }

    /// <summary>The resident step's rules: the step checks only the fields it shows.</summary>
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

    /// <summary>The care step's rules. Which care levels have an open room is known only to the server.</summary>
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

    /// <summary>The confirmation the server returns once the intake is saved.</summary>
    public sealed record IntakeConfirmation(string Confirmation);
}
