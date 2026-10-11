using Alis.Reactive.FluentValidator;
using FluentValidation;
using ResidentIntake.Models;

namespace ResidentIntake.Validators;

public class IntakeValidator : ReactiveValidator<ResidentIntakeModel>
{
    public IntakeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("'First Name' is required.")
            .MaximumLength(100).WithMessage("'First Name' must be at most 100 characters.");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("'Last Name' is required.")
            .MaximumLength(100).WithMessage("'Last Name' must be at most 100 characters.");
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("'Date of Birth' is required.");

        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("Please select a facility");
        RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Please select a care level");
        RuleFor(x => x.AdmissionDate).NotEmpty().WithMessage("'Admission Date' is required.");
        RuleFor(x => x.MonthlyRate)
            .NotEmpty().WithMessage("'Monthly Rate' is required.")
            .GreaterThan(0).WithMessage("Monthly rate must be greater than zero");

        RuleFor(x => x.EmergencyContactName)
            .NotEmpty().WithMessage("'Emergency Contact Name' is required.")
            .MaximumLength(100).WithMessage("'Emergency Contact Name' must be at most 100 characters.");
        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty().WithMessage("'Emergency Contact Phone' is required.")
            .MaximumLength(20).WithMessage("'Emergency Contact Phone' must be at most 20 characters.");

        When(x => x.RequiresMedicationManagement, () =>
        {
            RuleFor(x => x.PrimaryPhysician).NotEmpty().WithMessage("Physician is required when medication management is needed");
        });

        When(x => x.CareLevel == "memory-care", () =>
        {
            RuleFor(x => x.CognitiveAssessmentDate).NotEmpty().WithMessage("Cognitive assessment date is required for memory care residents");
        });

        // Rendered by the facility requirements partial after ResolvePlan joins it to the Active Plan.
        RuleFor(x => x.RoomPreference)
            .NotEmpty().WithMessage("Room preference is required for this facility")
            .MaximumLength(200).WithMessage("'Room Preference' must be at most 200 characters.");
        RuleFor(x => x.DepositAmount).NotEmpty().WithMessage("Move-in deposit is required");
    }
}
