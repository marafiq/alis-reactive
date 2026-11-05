namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DateValidationValidator : ReactiveValidator<DateValidationModel>
    {
        public DateValidationValidator()
        {
            RuleFor(x => x.AdmissionDate).NotEmpty().WithMessage("Admission date is required.");
            RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020, 1, 1)).WithMessage("Admission date must be on or after January 1, 2020.");
            RuleFor(x => x.DischargeDate).NotEmpty().WithMessage("Discharge date is required.");
            RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate).WithMessage("Discharge date must be after admission date.");

            RuleFor(x => x.CarePlanReviewDate)
                .GreaterThan(new DateTime(2024, 12, 31)).WithMessage("Date the care plan review after December 31, 2024.")
                .GreaterThanOrEqualTo(x => x.AdmissionDate).WithMessage("The care plan review cannot come before admission.")
                .LessThan(x => x.DischargeDate).WithMessage("The care plan review must come before discharge.");

            When(x => x.FollowUpVisitDate.HasValue, () =>
                RuleFor(x => x.FollowUpVisitDate)
                    .LessThanOrEqualTo(x => x.DischargeDate).WithMessage("The follow-up visit cannot come after discharge.")
                    .NotEqual(x => x.CarePlanReviewDate).WithMessage("Book the follow-up visit on a different day from the care plan review."));

            When(x => x.MoveOutInspectionDate.HasValue, () =>
                RuleFor(x => x.MoveOutInspectionDate)
                    .Equal(x => x.DischargeDate).WithMessage("The move-out inspection happens on the discharge day."));
        }
    }
}
