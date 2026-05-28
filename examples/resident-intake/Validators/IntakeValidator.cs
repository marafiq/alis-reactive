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
        ClientRule(x => x.FirstName)
            .Required("'First Name' is required.")
            .MaxLength(100, "'First Name' must be at most 100 characters.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        ClientRule(x => x.LastName)
            .Required("'Last Name' is required.")
            .MaxLength(100, "'Last Name' must be at most 100 characters.");
        RuleFor(x => x.DateOfBirth).NotEmpty();
        ClientRule(x => x.DateOfBirth)
            .Required("'Date of Birth' is required.");

        // Placement — always required
        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("Please select a facility");
        ClientRule(x => x.FacilityId)
            .Required("Please select a facility");
        RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Please select a care level");
        ClientRule(x => x.CareLevel)
            .Required("Please select a care level");
        RuleFor(x => x.AdmissionDate).NotEmpty();
        ClientRule(x => x.AdmissionDate)
            .Required("'Admission Date' is required.");
        RuleFor(x => x.MonthlyRate).NotEmpty()
            .GreaterThan(0).WithMessage("Monthly rate must be greater than zero");
        ClientRule(x => x.MonthlyRate)
            .Required("'Monthly Rate' is required.")
            .GreaterThan(0, "Monthly rate must be greater than zero");

        // Emergency Contact — always required
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(100);
        ClientRule(x => x.EmergencyContactName)
            .Required("'Emergency Contact Name' is required.")
            .MaxLength(100, "'Emergency Contact Name' must be at most 100 characters.");
        RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(20);
        ClientRule(x => x.EmergencyContactPhone)
            .Required("'Emergency Contact Phone' is required.")
            .MaxLength(20, "'Emergency Contact Phone' must be at most 20 characters.");

        // Conditional: medication management → physician required
        WhenField(x => x.RequiresMedicationManagement, () =>
        {
            RuleFor(x => x.PrimaryPhysician).NotEmpty()
                .WithMessage("Physician is required when medication management is needed");
            ClientRule(x => x.PrimaryPhysician)
                .Required("Physician is required when medication management is needed");
        });

        // Conditional: care level = memory-care → cognitive assessment required
        WhenField(x => x.CareLevel, "memory-care", () =>
        {
            RuleFor(x => x.CognitiveAssessmentDate).NotEmpty()
                .WithMessage("Cognitive assessment date is required for memory care residents");
            ClientRule(x => x.CognitiveAssessmentDate)
                .Required("Cognitive assessment date is required for memory care residents");
        });

        // Facility-specific fields (loaded via partial — unenriched until merge)
        RuleFor(x => x.RoomPreference).NotEmpty()
            .WithMessage("Room preference is required for this facility")
            .MaximumLength(200);
        ClientRule(x => x.RoomPreference)
            .Required("Room preference is required for this facility")
            .MaxLength(200, "'Room Preference' must be at most 200 characters.");
        RuleFor(x => x.DepositAmount).NotEmpty()
            .WithMessage("Move-in deposit is required");
        ClientRule(x => x.DepositAmount)
            .Required("Move-in deposit is required");
    }
}
