using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentProfileValidator : ReactiveValidator<ResidentProfile>
    {
        public ResidentProfileValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("'Name' is required.")
                .MaximumLength(80).WithMessage("'Name' must be at most 80 characters.");
            RuleFor(x => x.CareLevelId).NotEmpty().WithMessage("'Care Level' is required.");
            RuleFor(x => x.AdmissionDate)
                .NotEmpty().WithMessage("'Admission Date' is required.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("'Admission Date' must be at most today.");
            RuleFor(x => x.MonthlyRate)
                .GreaterThan(0m).WithMessage("'Monthly Rate' must be greater than 0.")
                .LessThanOrEqualTo(25000m).WithMessage("'Monthly Rate' must be at most 25000.");
            RuleFor(x => x.Nickname).MaximumLength(50).WithMessage("'Nickname' must be at most 50 characters.");
            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("'Date of Birth' is required.")
                .LessThan(DateTime.Today).WithMessage("'Date of Birth' must be before today.");
            RuleFor(x => x.Allergies).MaximumLength(200).WithMessage("'Allergies' must be at most 200 characters.");
        }
    }

    public class DateOfBirthQuickEditValidator : ReactiveValidator<DateOfBirthQuickEdit>
    {
        public DateOfBirthQuickEditValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("'Value' is required.")
                .LessThan(DateTime.Today).WithMessage("'Value' must be before today.");
        }
    }

    public class CareLevelQuickEditValidator : ReactiveValidator<CareLevelQuickEdit>
    {
        public CareLevelQuickEditValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("'Value' is required.")
                .MaximumLength(40).WithMessage("'Value' must be at most 40 characters.");
        }
    }

    public class MonthlyRateQuickEditValidator : ReactiveValidator<MonthlyRateQuickEdit>
    {
        public MonthlyRateQuickEditValidator()
        {
            RuleFor(x => x.Value)
                .GreaterThan(0m).WithMessage("'Value' must be greater than 0.")
                .LessThanOrEqualTo(25000m).WithMessage("'Value' must be at most 25000.");

            RuleFor(x => x.Value)
                .Must(rate => !IsAssignedToAnotherResidentInDemoStore(rate))
                .WithMessage("Monthly rate is already assigned to another resident (server-only check).");
        }

        private static bool IsAssignedToAnotherResidentInDemoStore(decimal rate) => rate == 7777m;
    }

    public class NicknameQuickEditValidator : ReactiveValidator<NicknameQuickEdit>
    {
        public NicknameQuickEditValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("'Value' is required.")
                .MaximumLength(50).WithMessage("'Value' must be at most 50 characters.");
        }
    }

    public class AllergiesQuickEditValidator : ReactiveValidator<AllergiesQuickEdit>
    {
        public AllergiesQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().WithMessage("Select at least one allergy.");
        }
    }

    public class LastAdmissionQuickEditValidator : ReactiveValidator<LastAdmissionQuickEdit>
    {
        public LastAdmissionQuickEditValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("'Value' is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("'Value' must be at most now.");
        }
    }

    public class MedicalRecordNumberQuickEditValidator : ReactiveValidator<MedicalRecordNumberQuickEdit>
    {
        public MedicalRecordNumberQuickEditValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("'Value' is required.")
                .Matches(@"^[A-Z]{3}\d{4}$").WithMessage("'Value' format is invalid.");
        }
    }
}
