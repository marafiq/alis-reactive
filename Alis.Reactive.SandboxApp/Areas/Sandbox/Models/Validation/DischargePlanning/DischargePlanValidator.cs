using System.Text.RegularExpressions;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The discharge plan's rules: each one applies under a condition on the answers around it, in every
    /// form a condition can take. The page decides each condition as the server does, as the answers change.
    /// </summary>
    public class DischargePlanValidator : ReactiveValidator<DischargePlanModel>
    {
        // The destinations that are transfers, once as an array and once as a list: an array's Contains
        // binds to a span in C# 14, a list's to the list, and the page reads the two differently today.
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

            // A yes/no answer read as it is, and read negated.
            When(x => x.GoingHome, () =>
                RuleFor(x => x.HomeAddress).NotEmpty().WithMessage("Enter the home address the resident goes to."));
            When(x => !x.GoingHome, () =>
                RuleFor(x => x.HomeAddress).Empty().WithMessage("Only a resident going home has a home address to record."));

            // Both answers at once.
            When(x => x.GoingHome && x.LivesAlone, () =>
                RuleFor(x => x.DailyCheckInBy).NotEmpty().WithMessage("Name who checks in daily on a resident living alone."));

            // A condition inside a condition: the inner one counts only while the outer one holds.
            When(x => x.GoingHome, () =>
            {
                When(x => x.HasPets, () =>
                    RuleFor(x => x.PetCarePlan).NotEmpty().WithMessage("Say who cares for the pets."));
            });

            // A number left blank, and a number given.
            When(x => x.FollowUpDays == null, () =>
                RuleFor(x => x.FollowUpClinic).Empty().WithMessage("A clinic is named only when a follow-up is due."));
            When(x => x.FollowUpDays.HasValue, () =>
                RuleFor(x => x.FollowUpClinic).NotEmpty().WithMessage("Name the clinic for the follow-up."));

            // Text that matches a pattern.
            When(x => x.FollowUpClinic != null && Regex.IsMatch(x.FollowUpClinic, @"^VA "), () =>
                RuleFor(x => x.VaCaseNumber).NotEmpty().WithMessage("A VA clinic needs the veteran's case number."));

            // Text left empty, and text given.
            When(x => string.IsNullOrEmpty(x.PrimaryCaregiver), () =>
                RuleFor(x => x.CaregiverPhone).Empty().WithMessage("A caregiver phone needs a caregiver."));
            When(x => !string.IsNullOrEmpty(x.PrimaryCaregiver), () =>
                RuleFor(x => x.CaregiverPhone).NotEmpty().WithMessage("Enter the caregiver's phone."));

            // A choice equal to one value, one of several values, and anything but one value.
            When(x => x.Destination == DischargeDestination.SkilledNursing, () =>
                RuleFor(x => x.BedConfirmation).NotEmpty().WithMessage("Enter the bed confirmation from the nursing facility."));
            When(x => TransferDestinations.Contains(x.Destination), () =>
                RuleFor(x => x.TransferSummary).NotEmpty().WithMessage("A transfer needs the nursing summary."));
            When(x => TransferDestinationList.Contains(x.Destination), () =>
                RuleFor(x => x.ReceivingNurse).NotEmpty().WithMessage("Name the nurse receiving the resident."));
            When(x => !(x.Destination == DischargeDestination.Home), () =>
                RuleFor(x => x.HomeHealthAgency).Empty().WithMessage("Only a resident going home has a home health agency."));

            // Unless, with what applies otherwise.
            Unless(x => x.NeedsTransport, () =>
                    RuleFor(x => x.TransportCompany).Empty().WithMessage("No transport is booked for a resident the family drives."))
                .Otherwise(() =>
                    RuleFor(x => x.TransportCompany).NotEmpty().WithMessage("Name the transport company."));

            // A number compared, and either of two numbers compared.
            When(x => x.StairsAtHome > 0, () =>
                RuleFor(x => x.StairPlan).NotEmpty().WithMessage("Say how the resident manages the stairs."));
            When(x => x.FallsLast90Days > 1 || x.StairsAtHome > 2, () =>
                RuleFor(x => x.HomeSafetyVisit).NotNull().WithMessage("Book a home safety visit and enter its date."));

            // Text that contains, starts with and ends with something.
            When(x => x.Medications != null && x.Medications.Contains("warfarin"), () =>
                RuleFor(x => x.InrCheckDate).NotNull().WithMessage("Warfarin needs the date of the next INR check."));
            When(x => x.DiagnosisCode != null && x.DiagnosisCode.StartsWith("F0"), () =>
                RuleFor(x => x.MemorySupportPlan).NotEmpty().WithMessage("A dementia diagnosis needs a memory support plan."));
            When(x => x.RoomCode != null && x.RoomCode.EndsWith("-ISO"), () =>
                RuleFor(x => x.IsolationClearedBy).NotEmpty().WithMessage("Name who cleared the resident from isolation."));

            // A condition written after a rule's checks.
            RuleFor(x => x.Medications)
                .MaximumLength(60).WithMessage("A list this long needs a pharmacist's review: tick it.")
                .Unless(x => x.PharmacistReviewed);

            // A list that holds an item.
            When(x => x.Equipment.Contains("Oxygen"), () =>
                RuleFor(x => x.OxygenVendor).NotEmpty().WithMessage("Name the oxygen vendor."));
        }
    }
}
