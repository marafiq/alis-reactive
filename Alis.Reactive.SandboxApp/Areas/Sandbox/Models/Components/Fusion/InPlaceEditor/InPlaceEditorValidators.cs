using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentProfileValidator : ReactiveValidator<ResidentProfile>
    {
        public ResidentProfileValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            ClientRule(x => x.Name)
                .Required("'Name' is required.")
                .MaxLength(80, "'Name' must be at most 80 characters.");
            RuleFor(x => x.CareLevelId).NotEmpty();
            ClientRule(x => x.CareLevelId)
                .Required("'Care Level' is required.");
            RuleFor(x => x.AdmissionDate).NotNull().LessThanOrEqualTo(DateTime.Today);
            ClientRule(x => x.AdmissionDate)
                .Required("'Admission Date' is required.")
                .LessThanOrEqualTo(DateTime.Today, "'Admission Date' must be at most today.");
            RuleFor(x => x.MonthlyRate).GreaterThan(0m).LessThanOrEqualTo(25000m);
            ClientRule(x => x.MonthlyRate)
                .GreaterThan(0m, "'Monthly Rate' must be greater than 0.")
                .LessThanOrEqualTo(25000m, "'Monthly Rate' must be at most 25000.");
            RuleFor(x => x.Nickname).MaximumLength(50);
            ClientRule(x => x.Nickname)
                .MaxLength(50, "'Nickname' must be at most 50 characters.");
            RuleFor(x => x.DateOfBirth).NotNull().LessThan(DateTime.Today);
            ClientRule(x => x.DateOfBirth)
                .Required("'Date of Birth' is required.")
                .LessThan(DateTime.Today, "'Date of Birth' must be before today.");
            RuleFor(x => x.Allergies).MaximumLength(200);
            ClientRule(x => x.Allergies)
                .MaxLength(200, "'Allergies' must be at most 200 characters.");
        }
    }

    public class DateOfBirthQuickEditValidator : ReactiveValidator<DateOfBirthQuickEdit>
    {
        public DateOfBirthQuickEditValidator()
        {
            RuleFor(x => x.Value).NotNull().LessThan(DateTime.Today);
            ClientRule(x => x.Value)
                .Required("'Value' is required.")
                .LessThan(DateTime.Today, "'Value' must be before today.");
        }
    }

    public class CareLevelQuickEditValidator : ReactiveValidator<CareLevelQuickEdit>
    {
        public CareLevelQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(40);
            ClientRule(x => x.Value)
                .Required("'Value' is required.")
                .MaxLength(40, "'Value' must be at most 40 characters.");
        }
    }

    public class MonthlyRateQuickEditValidator : ReactiveValidator<MonthlyRateQuickEdit>
    {
        public MonthlyRateQuickEditValidator()
        {
            RuleFor(x => x.Value).GreaterThan(0m).LessThanOrEqualTo(25000m);
            ClientRule(x => x.Value)
                .GreaterThan(0m, "'Value' must be greater than 0.")
                .LessThanOrEqualTo(25000m, "'Value' must be at most 25000.");

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
            RuleFor(x => x.Value).NotEmpty().MaximumLength(50);
            ClientRule(x => x.Value)
                .Required("'Value' is required.")
                .MaxLength(50, "'Value' must be at most 50 characters.");
        }
    }

    public class AllergiesQuickEditValidator : ReactiveValidator<AllergiesQuickEdit>
    {
        public AllergiesQuickEditValidator()
        {
            RuleFor(x => x.Value).NotNull();
            ClientRule(x => x.Value)
                .AtLeastOne("Select at least one allergy.");
        }
    }

    public class LastAdmissionQuickEditValidator : ReactiveValidator<LastAdmissionQuickEdit>
    {
        public LastAdmissionQuickEditValidator()
        {
            RuleFor(x => x.Value).NotNull().LessThanOrEqualTo(DateTime.UtcNow);
            ClientRule(x => x.Value)
                .Required("'Value' is required.")
                .LessThanOrEqualTo(DateTime.UtcNow, "'Value' must be at most now.");
        }
    }

    public class MedicalRecordNumberQuickEditValidator : ReactiveValidator<MedicalRecordNumberQuickEdit>
    {
        public MedicalRecordNumberQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().Matches(@"^[A-Z]{3}\d{4}$");
            ClientRule(x => x.Value)
                .Required("'Value' is required.")
                .Regex(@"^[A-Z]{3}\d{4}$", "'Value' format is invalid.");
        }
    }
}
