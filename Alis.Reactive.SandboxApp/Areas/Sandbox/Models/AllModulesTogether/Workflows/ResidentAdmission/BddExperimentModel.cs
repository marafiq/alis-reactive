using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.AllModulesTogether.Workflows.ResidentAdmission
{
    public class BddExperimentModel
    {
        public string? ResidentName { get; set; }
        public string? Physician { get; set; }
        public string? CareLevel { get; set; }
        public decimal? MonthlyRate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class CareLevelOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class BddExperimentValidator : AbstractValidator<BddExperimentModel>
    {
        public BddExperimentValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Physician is required.");
            RuleFor(x => x.MonthlyRate).NotNull().WithMessage("Monthly rate is required.");
        }
    }

    public class PhysicianOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    public class BddExperimentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
