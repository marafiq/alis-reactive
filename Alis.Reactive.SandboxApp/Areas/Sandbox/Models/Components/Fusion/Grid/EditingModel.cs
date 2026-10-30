using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A resident on the Maple wing roster, one row of the roster grid.</summary>
    public class MapleRosterResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }
        public string PrimaryNurse { get; set; } = "";
    }

    /// <summary>A resident's row as the handoff correction left it, and what the correction did.</summary>
    public class MapleRosterCorrected
    {
        public MapleRosterResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>
    /// The coordinator's rules for a correction from the night nurse's handoff note: whom it is for, the
    /// resident's risk, and the open tasks the night shift can carry. The server also checks the resident
    /// lives on the wing.
    /// </summary>
    public class MapleRosterCorrectionValidator : ReactiveValidator<ResidentGridEditingModel>
    {
        public MapleRosterCorrectionValidator()
        {
            RuleFor(x => x.ResidentName)
                .NotEmpty().WithMessage("Name the resident to correct.")
                .MinimumLength(3).WithMessage("Write the resident's full name.");

            RuleFor(x => x.RiskLevel)
                .NotEmpty().WithMessage("Choose the resident's risk level.")
                .Must(risk => risk is "Low" or "Moderate" or "High").WithMessage("Choose Low, Moderate or High.");

            // No open tasks is a count the note can give, so only a missing count is refused.
            RuleFor(x => x.OpenTasks)
                .NotNull().WithMessage("Enter the open tasks from the handoff note.")
                .InclusiveBetween(0m, 7m).WithMessage("The night shift carries 0 to 7 open tasks per resident.");
        }
    }
}
