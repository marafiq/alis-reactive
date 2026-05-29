using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
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

    public class BddExperimentValidator : ReactiveValidator<BddExperimentModel>
    {
        public BddExperimentValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");
            ClientRule(x => x.ResidentName).Required("Resident name is required.");
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Physician is required.");
            ClientRule(x => x.Physician).Required("Physician is required.");
            RuleFor(x => x.MonthlyRate).NotNull().WithMessage("Monthly rate is required.");
            ClientRule(x => x.MonthlyRate).Required("Monthly rate is required.");
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
