using Alis.Reactive.FluentValidator;
using FluentValidation;
using ResidentIntake.Models;

namespace ResidentIntake.Validators;

public class IntakeValidator : ReactiveValidator<ResidentIntakeModel>
{
    public IntakeValidator()
    {
        // Personal Info — always required
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty();

        // Placement — always required
        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("Please select a facility");
        RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Please select a care level");
        RuleFor(x => x.AdmissionDate).NotEmpty();
        RuleFor(x => x.MonthlyRate).NotEmpty()
            .GreaterThan(0).WithMessage("Monthly rate must be greater than zero");

        // Emergency Contact — always required
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(20);

        // Conditional: medication management → physician required
        WhenField(x => x.RequiresMedicationManagement, () =>
        {
            RuleFor(x => x.PrimaryPhysician).NotEmpty()
                .WithMessage("Physician is required when medication management is needed");
        });

        // Conditional: care level = memory-care → cognitive assessment required
        WhenField(x => x.CareLevel, "memory-care", () =>
        {
            RuleFor(x => x.CognitiveAssessmentDate).NotEmpty()
                .WithMessage("Cognitive assessment date is required for memory care residents");
        });

        // Facility-specific fields (loaded via partial — unenriched until merge)
        RuleFor(x => x.RoomPreference).NotEmpty()
            .WithMessage("Room preference is required for this facility")
            .MaximumLength(200);
        RuleFor(x => x.DepositAmount).NotEmpty()
            .WithMessage("Move-in deposit is required");
    }
}
