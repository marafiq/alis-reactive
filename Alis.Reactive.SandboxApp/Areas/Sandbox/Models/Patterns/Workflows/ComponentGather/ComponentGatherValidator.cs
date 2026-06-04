using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ComponentGatherValidator : ReactiveValidator<ComponentGatherModel>
    {
        public ComponentGatherValidator()
        {
            RuleFor(x => x.ResidentName)
                .NotEmpty().WithMessage("'Resident Name' is required.")
                .MinimumLength(3).WithMessage("'Resident Name' must be at least 3 characters.")
                .MaximumLength(100).WithMessage("'Resident Name' must be at most 100 characters.");
            ClientRule(x => x.ResidentName)
                .Required("'Resident Name' is required.")
                .MinLength(3, "'Resident Name' must be at least 3 characters.")
                .MaxLength(100, "'Resident Name' must be at most 100 characters.");
            RuleFor(x => x.CareNotes).NotEmpty().WithMessage("'Care Notes' is required.");
            ClientRule(x => x.CareNotes).Required("'Care Notes' is required.");
            RuleFor(x => x.MobilityLevel).NotEmpty().WithMessage("'Mobility Level' is required.");
            ClientRule(x => x.MobilityLevel).Required("'Mobility Level' is required.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");
            ClientRule(x => x.CareLevel).Required("'Care Level' is required.");

            RuleFor(x => x.Allergies).NotEmpty().WithMessage("Select at least one allergy.");
            ClientRule(x => x.Allergies).AtLeastOne("Select at least one allergy.");

            RuleFor(x => x.MonthlyRate).GreaterThan(0).WithMessage("'Monthly Rate' must be greater than 0.");
            ClientRule(x => x.MonthlyRate).GreaterThan(0, "'Monthly Rate' must be greater than 0.");
            RuleFor(x => x.FacilityId).NotEmpty().WithMessage("'Facility' is required.");
            ClientRule(x => x.FacilityId).Required("'Facility' is required.");
            RuleFor(x => x.PhysicianName).NotEmpty().WithMessage("'Physician Name' is required.");
            ClientRule(x => x.PhysicianName).Required("'Physician Name' is required.");
            RuleFor(x => x.AdmissionDate).NotEmpty().WithMessage("'Admission Date' is required.");
            ClientRule(x => x.AdmissionDate).Required("'Admission Date' is required.");
            RuleFor(x => x.MedicationTime).NotEmpty().WithMessage("'Medication Time' is required.");
            ClientRule(x => x.MedicationTime).Required("'Medication Time' is required.");
            RuleFor(x => x.AppointmentTime).NotEmpty().WithMessage("'Appointment Time' is required.");
            ClientRule(x => x.AppointmentTime).Required("'Appointment Time' is required.");
            RuleFor(x => x.StayPeriod).NotEmpty().WithMessage("'Stay Period' is required.");
            ClientRule(x => x.StayPeriod).Required("'Stay Period' is required.");
            RuleFor(x => x.InsuranceProvider).NotEmpty().WithMessage("'Insurance Provider' is required.");
            ClientRule(x => x.InsuranceProvider).Required("'Insurance Provider' is required.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("'Phone Number' is required.");
            ClientRule(x => x.PhoneNumber).Required("'Phone Number' is required.");
            RuleFor(x => x.CarePlan).NotEmpty().WithMessage("'Care Plan' is required.");
            ClientRule(x => x.CarePlan).Required("'Care Plan' is required.");

            RuleFor(x => x.DietaryRestrictions).NotEmpty().WithMessage("Select at least one dietary restriction.");
            ClientRule(x => x.DietaryRestrictions).AtLeastOne("Select at least one dietary restriction.");
        }
    }
}
