using System.Text.RegularExpressions;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DischargePlanValidator : ReactiveValidator<DischargePlanModel>
    {
        private static readonly DischargeDestination?[] TransferDestinations =
        {
            DischargeDestination.SkilledNursing,
            DischargeDestination.Hospice,
        };

        private static readonly List<DischargeDestination?> TransferDestinationList = new()
        {
            DischargeDestination.SkilledNursing,
            DischargeDestination.Hospice,
        };

        public DischargePlanValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");

            When(x => x.GoingHome, () =>
                RuleFor(x => x.HomeAddress).NotEmpty().WithMessage("Enter the home address the resident goes to."));
            When(x => !x.GoingHome, () =>
                RuleFor(x => x.HomeAddress).Empty().WithMessage("Only a resident going home has a home address to record."));

            When(x => x.GoingHome && x.LivesAlone, () =>
                RuleFor(x => x.DailyCheckInBy).NotEmpty().WithMessage("Name who checks in daily on a resident living alone."));

            When(x => x.GoingHome, () =>
            {
                When(x => x.HasPets, () =>
                    RuleFor(x => x.PetCarePlan).NotEmpty().WithMessage("Say who cares for the pets."));
            });

            When(x => x.FollowUpDays == null, () =>
                RuleFor(x => x.FollowUpClinic).Empty().WithMessage("A clinic is named only when a follow-up is due."));
            When(x => x.FollowUpDays.HasValue, () =>
                RuleFor(x => x.FollowUpClinic).NotEmpty().WithMessage("Name the clinic for the follow-up."));

            When(x => x.FollowUpClinic != null && Regex.IsMatch(x.FollowUpClinic, @"^VA "), () =>
                RuleFor(x => x.VaCaseNumber).NotEmpty().WithMessage("A VA clinic needs the veteran's case number."));

            When(x => string.IsNullOrEmpty(x.PrimaryCaregiver), () =>
                RuleFor(x => x.CaregiverPhone).Empty().WithMessage("A caregiver phone needs a caregiver."));
            When(x => !string.IsNullOrEmpty(x.PrimaryCaregiver), () =>
                RuleFor(x => x.CaregiverPhone).NotEmpty().WithMessage("Enter the caregiver's phone."));

            When(x => x.Destination == DischargeDestination.SkilledNursing, () =>
                RuleFor(x => x.BedConfirmation).NotEmpty().WithMessage("Enter the bed confirmation from the nursing facility."));
            When(x => TransferDestinations.Contains(x.Destination), () =>
                RuleFor(x => x.TransferSummary).NotEmpty().WithMessage("A transfer needs the nursing summary."));
            When(x => TransferDestinationList.Contains(x.Destination), () =>
                RuleFor(x => x.ReceivingNurse).NotEmpty().WithMessage("Name the nurse receiving the resident."));
            When(x => !(x.Destination == DischargeDestination.Home), () =>
                RuleFor(x => x.HomeHealthAgency).Empty().WithMessage("Only a resident going home has a home health agency."));

            Unless(x => x.NeedsTransport, () =>
                    RuleFor(x => x.TransportCompany).Empty().WithMessage("No transport is booked for a resident the family drives."))
                .Otherwise(() =>
                    RuleFor(x => x.TransportCompany).NotEmpty().WithMessage("Name the transport company."));

            When(x => x.StairsAtHome > 0, () =>
                RuleFor(x => x.StairPlan).NotEmpty().WithMessage("Say how the resident manages the stairs."));
            When(x => x.FallsLast90Days > 1 || x.StairsAtHome > 2, () =>
                RuleFor(x => x.HomeSafetyVisit).NotNull().WithMessage("Book a home safety visit and enter its date."));

            When(x => x.Medications != null && x.Medications.Contains("warfarin"), () =>
                RuleFor(x => x.InrCheckDate).NotNull().WithMessage("Warfarin needs the date of the next INR check."));
            When(x => x.DiagnosisCode != null && x.DiagnosisCode.StartsWith("F0"), () =>
                RuleFor(x => x.MemorySupportPlan).NotEmpty().WithMessage("A dementia diagnosis needs a memory support plan."));
            When(x => x.RoomCode != null && x.RoomCode.EndsWith("-ISO"), () =>
                RuleFor(x => x.IsolationClearedBy).NotEmpty().WithMessage("Name who cleared the resident from isolation."));

            RuleFor(x => x.Medications)
                .MaximumLength(60).WithMessage("A list this long needs a pharmacist's review: tick it.")
                .Unless(x => x.PharmacistReviewed);

            When(x => x.Equipment.Contains("Oxygen"), () =>
                RuleFor(x => x.OxygenVendor).NotEmpty().WithMessage("Name the oxygen vendor."));
        }
    }
}
