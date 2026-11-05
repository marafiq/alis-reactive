using System.Text.RegularExpressions;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MoveInReviewValidator : ReactiveValidator<MoveInReviewModel>
    {
        public MoveInReviewValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty();

            RuleFor(x => x.ResidentName).MaximumLength(30)
                .WithMessage(review => $"{review.ResidentName} is longer than the 30 characters a wristband prints.")
                .Must(name => name == null || !name.Any(char.IsDigit))
                .WithMessage("A wristband prints letters only: remove the digits from the name.");

            RuleFor(x => x.FallRiskScore)
                .NotNull().WithMessage("Score the resident's fall risk.")
                .InclusiveBetween(0, 10).WithMessage("Fall risk is scored 0 to 10.");

            When(x => x.FallRiskScore.HasValue && x.FallRiskScore.Value > 7, () =>
                    RuleFor(x => x.MobilityNotes).NotEmpty().WithMessage("Describe the mobility plan for a high fall risk."))
                .Otherwise(() =>
                    RuleFor(x => x.MobilityNotes).MaximumLength(40).WithMessage("Keep mobility notes under 40 characters."));

            RuleFor(x => x.ReassessmentPlan).NotEmpty().WithMessage("A moderate fall risk needs a reassessment plan.")
                .When(x => x.FallRiskScore >= 5 && x.FallRiskScore <= 7)
                .MaximumLength(120).WithMessage("Keep the reassessment plan under 120 characters.");

            When(x => x.MobilityNotes != null && x.MobilityNotes.Length > 60, () =>
                RuleFor(x => x.TherapistReviewer).NotEmpty().WithMessage("A mobility plan this long needs a therapist to review it."));

            When(x => x.UsesWheelchair || x.UsesOxygen, () =>
                RuleFor(x => x.EvacuationAssist).NotEmpty().WithMessage("Name who helps the resident out in an evacuation."));

            RuleFor(x => x.AssignedRoom)
                .Matches(@"^[a-z]{2}-\d{3}$", RegexOptions.IgnoreCase).WithMessage("Enter the room as wing and number, like MA-214.");

            When(x => x.AssignedRoom != null && x.AssignedRoom.StartsWith("MC-"), () =>
                RuleFor(x => x.SecuredUnitConsentBy).NotEmpty().WithMessage("Name the family member who consented to the secured unit."));

            Unless(x => string.IsNullOrEmpty(x.FamilyPortalLink), () =>
                RuleFor(x => x.FamilyPortalLink).Url().WithMessage("The family portal link must be a web address."));

            RuleFor(x => x.WristbandCode).WristbandCode();

            When(x => x.WristbandCode != null && Regex.IsMatch(x.WristbandCode, @"^WB-9\d{5}$"), () =>
                RuleFor(x => x.TemporaryBandReason).NotEmpty().WithMessage("Say why the resident wears a temporary wristband."));

            Unless(x => string.IsNullOrEmpty(x.Pharmacy.Phone), () =>
                RuleFor(x => x.Pharmacy.Name).NotEmpty().WithMessage("Name the pharmacy for this phone number."));

            When(x => x.MoveInDate.HasValue, () =>
                RuleFor(x => x.Pharmacy.FirstDelivery)
                    .GreaterThanOrEqualTo(x => x.MoveInDate).WithMessage("The pharmacy's first delivery cannot come before the move-in day."));

            RuleFor(x => x.Pharmacy).ChildRules(pharmacy =>
                pharmacy.RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{4}$").WithMessage("Enter the pharmacy phone as 555-0100."));

            RuleFor(x => x.ForwardingZip)
                .Matches(@"^\d{5}\z").WithMessage("Enter the forwarding ZIP as five digits.");

            RuleFor(x => x.CarePlanSummary).MaximumLength(500).WithMessage("Keep the care plan summary under 500 characters.");

            WhenOnServer(x => IsReturningResident(x), () =>
                RuleFor(x => x.PreviousRoom).NotEmpty().WithMessage("A returning resident's previous room is required."));

            RuleFor(x => x.ReviewedBy).NotEmpty().ServerOnly();
        }

        private static bool IsReturningResident(MoveInReviewModel review) => review.IsReturning;
    }

    public static class WristbandRules
    {
        public static IRuleBuilderOptions<T, string?> WristbandCode<T>(this IRuleBuilder<T, string?> rule)
            => rule.Matches(@"^WB-\d{6}$").WithMessage("Enter the wristband code as WB- and six digits, like WB-104233.");
    }
}
