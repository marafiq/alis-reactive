using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DateValidationValidator : AbstractValidator<DateValidationModel>
    {
        public DateValidationValidator()
        {
            RuleFor(x => x.AdmissionDate).NotEmpty()
                .WithMessage("Admission date is required.");
            RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1))
                .WithMessage("Admission date must be on or after January 1, 2020.");
            RuleFor(x => x.DischargeDate).NotEmpty()
                .WithMessage("Discharge date is required.");
            RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)
                .WithMessage("Discharge date must be after admission date.");
        }
    }
}
