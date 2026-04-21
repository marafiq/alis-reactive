using System;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentProfileValidator : AbstractValidator<ResidentProfile>
    {
        public ResidentProfileValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
            RuleFor(x => x.CareLevelId).NotEmpty();
            RuleFor(x => x.AdmissionDate).NotNull().LessThanOrEqualTo(DateTime.Today);
            RuleFor(x => x.MonthlyRate).GreaterThan(0m).LessThanOrEqualTo(25000m);
            RuleFor(x => x.Nickname).MaximumLength(50);
            RuleFor(x => x.DateOfBirth).NotNull().LessThan(DateTime.Today);
            RuleFor(x => x.Allergies).MaximumLength(200);
        }
    }

    public class DateOfBirthQuickEditValidator : AbstractValidator<DateOfBirthQuickEdit>
    {
        public DateOfBirthQuickEditValidator()
        {
            RuleFor(x => x.Value).NotNull().LessThan(DateTime.Today);
        }
    }

    public class CareLevelQuickEditValidator : AbstractValidator<CareLevelQuickEdit>
    {
        public CareLevelQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(40);
        }
    }

    public class MonthlyRateQuickEditValidator : AbstractValidator<MonthlyRateQuickEdit>
    {
        public MonthlyRateQuickEditValidator()
        {
            RuleFor(x => x.Value).GreaterThan(0m).LessThanOrEqualTo(25000m);
        }
    }

    public class NicknameQuickEditValidator : AbstractValidator<NicknameQuickEdit>
    {
        public NicknameQuickEditValidator()
        {
            RuleFor(x => x.Value).NotEmpty().MaximumLength(50);
        }
    }

    public class AllergiesQuickEditValidator : AbstractValidator<AllergiesQuickEdit>
    {
        public AllergiesQuickEditValidator()
        {
            // At least one allergy selected (MultiSelect returns string[]).
            RuleFor(x => x.Value).NotNull();
        }
    }

    public class LastAdmissionQuickEditValidator : AbstractValidator<LastAdmissionQuickEdit>
    {
        public LastAdmissionQuickEditValidator()
        {
            RuleFor(x => x.Value).NotNull().LessThanOrEqualTo(DateTime.UtcNow);
        }
    }

    public class MedicalRecordNumberQuickEditValidator : AbstractValidator<MedicalRecordNumberQuickEdit>
    {
        public MedicalRecordNumberQuickEditValidator()
        {
            // Syncfusion Mask exposes `value` with mask literals stripped — the dash is
            // display-only. Domain value is 3 letters + 4 digits (e.g. "MRN1234"),
            // rendered as "MRN-1234" through the LLL-0000 mask.
            RuleFor(x => x.Value).NotEmpty().Matches(@"^[A-Z]{3}\d{4}$");
        }
    }
}
