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
            // Client-extractable rules — shape the client adapter recognizes (GreaterThan + LessThanOrEqualTo)
            // and runs before POST.
            RuleFor(x => x.Value).GreaterThan(0m).LessThanOrEqualTo(25000m);

            // Server-only rule — Must() becomes a PredicateValidator which the FluentValidationAdapter
            // does not extract, so the client never sees it. Simulates "is this rate already in use by
            // another resident?" style DB-gated invariants. Enter 7777 in the browser to trigger it:
            // client will let the POST through, the server will reject with the framework-standard
            // { errors: { Value: [...] } } shape, and .OnError(400, e => e.ValidationErrors(formId))
            // renders the message in the per-field slot.
            RuleFor(x => x.Value)
                .Must(rate => !IsAlreadyAssignedToAnotherResident(rate))
                .WithMessage("Monthly rate is already assigned to another resident (server-only check).");
        }

        // Pretend this reaches a database. In production this would be a repository call.
        private static bool IsAlreadyAssignedToAnotherResident(decimal rate) => rate == 7777m;
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
