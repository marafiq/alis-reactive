using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ComponentGatherValidator : ReactiveValidator<ComponentGatherModel>
    {
        public ComponentGatherValidator()
        {
            // Native scalar — multiple rules on ResidentName
            RuleFor(x => x.ResidentName)
                .NotEmpty().WithMessage("'Resident Name' is required.")
                .MinimumLength(3).WithMessage("'Resident Name' must be at least 3 characters.")
                .MaximumLength(100).WithMessage("'Resident Name' must be at most 100 characters.");
            RuleFor(x => x.CareNotes).NotEmpty().WithMessage("'Care Notes' is required.");
            RuleFor(x => x.MobilityLevel).NotEmpty().WithMessage("'Mobility Level' is required.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

            // Native array
            RuleFor(x => x.Allergies).NotEmpty().WithMessage("Select at least one allergy.");

            // Fusion scalar
            RuleFor(x => x.MonthlyRate).GreaterThan(0).WithMessage("'Monthly Rate' must be greater than 0.");
            RuleFor(x => x.FacilityId).NotEmpty().WithMessage("'Facility' is required.");
            RuleFor(x => x.PhysicianName).NotEmpty().WithMessage("'Physician Name' is required.");
            RuleFor(x => x.AdmissionDate).NotEmpty().WithMessage("'Admission Date' is required.");
            RuleFor(x => x.MedicationTime).NotEmpty().WithMessage("'Medication Time' is required.");
            RuleFor(x => x.AppointmentTime).NotEmpty().WithMessage("'Appointment Time' is required.");
            RuleFor(x => x.StayPeriod).NotEmpty().WithMessage("'Stay Period' is required.");
            RuleFor(x => x.InsuranceProvider).NotEmpty().WithMessage("'Insurance Provider' is required.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("'Phone Number' is required.");
            RuleFor(x => x.CarePlan).NotEmpty().WithMessage("'Care Plan' is required.");

            // Fusion array
            RuleFor(x => x.DietaryRestrictions).NotEmpty().WithMessage("Select at least one dietary restriction.");
        }
    }
}
