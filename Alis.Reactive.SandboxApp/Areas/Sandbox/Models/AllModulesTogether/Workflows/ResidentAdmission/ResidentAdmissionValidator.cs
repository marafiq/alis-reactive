using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentAdmissionValidator : AbstractValidator<ResidentAdmissionModel>
    {
        public ResidentAdmissionValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Physician is required.");
            RuleFor(x => x.MonthlyRate).NotNull().WithMessage("Monthly rate is required.");
        }
    }
}
