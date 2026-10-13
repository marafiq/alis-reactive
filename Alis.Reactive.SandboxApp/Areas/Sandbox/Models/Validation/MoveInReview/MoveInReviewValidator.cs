using Alis.Reactive.FluentValidator;
using System.Text.RegularExpressions;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Written once, in FluentValidation's grammar. The browser runs every rule it can evaluate, under
    /// the same conditions; the rest run on the server only.
    /// </summary>
    public class MoveInReviewValidator : ReactiveValidator<MoveInReviewModel>
    {
        public MoveInReviewValidator()
        {
            // The grammar's own message, naming the field the way FluentValidation names it.
            RuleFor(x => x.ResidentName).NotEmpty();

            // A message written from the model can only be said by the server, so this rule runs there;
            // so does the printer's letters-only check. A name can break both, and the server says both.
            RuleFor(x => x.ResidentName).MaximumLength(30)
                .WithMessage(review => $"{review.ResidentName} is longer than the 30 characters a wristband prints.")
                .Must(name => name == null || !name.Any(char.IsDigit))
                .WithMessage("A wristband prints letters only: remove the digits from the name.");

            RuleFor(x => x.FallRiskScore)
                .NotNull().WithMessage("Score the resident's fall risk.")
                .InclusiveBetween(0, 10).WithMessage("Fall risk is scored 0 to 10.");

            // A nullable member reads as C# writes it: HasValue, then Value.
            When(x => x.FallRiskScore.HasValue && x.FallRiskScore.Value > 7, () =>
                    RuleFor(x => x.MobilityNotes).NotEmpty().WithMessage("Describe the mobility plan for a high fall risk."))
                .Otherwise(() =>
                    RuleFor(x => x.MobilityNotes).MaximumLength(40).WithMessage("Keep mobility notes under 40 characters."));

            // A moderate score, 5 to 7, needs a plan for reassessing the resident. A condition written after
            // a rule's checks conditions them in the browser too.
            RuleFor(x => x.ReassessmentPlan).NotEmpty().WithMessage("A moderate fall risk needs a reassessment plan.")
                .When(x => x.FallRiskScore >= 5 && x.FallRiskScore <= 7);

            // A long mobility plan goes to a therapist. A condition on a length has no browser form,
            // so the server alone checks this rule.
            When(x => x.MobilityNotes != null && x.MobilityNotes.Length > 60, () =>
                RuleFor(x => x.TherapistReviewer).NotEmpty().WithMessage("A mobility plan this long needs a therapist to review it."));

            // Either need means someone must help the resident out in an evacuation.
            When(x => x.UsesWheelchair || x.UsesOxygen, () =>
                RuleFor(x => x.EvacuationAssist).NotEmpty().WithMessage("Name who helps the resident out in an evacuation."));

            // RegexOptions travel with the pattern: "ma-214" and "MA-214" are the same room.
            RuleFor(x => x.AssignedRoom)
                .Matches(@"^[a-z]{2}-\d{3}$", RegexOptions.IgnoreCase).WithMessage("Enter the room as wing and number, like MA-214.");

            // Rooms on the secured memory care wing are numbered MC-.
            When(x => x.AssignedRoom != null && x.AssignedRoom.StartsWith("MC-"), () =>
                RuleFor(x => x.SecuredUnitConsentBy).NotEmpty().WithMessage("Name the family member who consented to the secured unit."));

            Unless(x => string.IsNullOrEmpty(x.FamilyPortalLink), () =>
                RuleFor(x => x.FamilyPortalLink).Url().WithMessage("The family portal link must be a web address."));

            // The community's own check, composed from the grammar's, so the browser runs it too.
            RuleFor(x => x.WristbandCode).WristbandCode();

            // Temporary wristbands are numbered WB-9 and five digits.
            When(x => x.WristbandCode != null && Regex.IsMatch(x.WristbandCode, @"^WB-9\d{5}$"), () =>
                RuleFor(x => x.TemporaryBandReason).NotEmpty().WithMessage("Say why the resident wears a temporary wristband."));

            // A pharmacy phone needs the pharmacy it reaches.
            Unless(x => string.IsNullOrEmpty(x.Pharmacy.Phone), () =>
                RuleFor(x => x.Pharmacy.Name).NotEmpty().WithMessage("Name the pharmacy for this phone number."));

            // Rules for a nested object, written inline.
            RuleFor(x => x.Pharmacy).ChildRules(pharmacy =>
                pharmacy.RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{4}$").WithMessage("Enter the pharmacy phone as 555-0100."));

            // \z is .NET's end of input; JavaScript reads it as a letter z, so the server checks this pattern.
            RuleFor(x => x.ForwardingZip)
                .Matches(@"^\d{5}\z").WithMessage("Enter the forwarding ZIP as five digits.");

            // A field another page edits: this page sends no summary, which is under 500 characters.
            RuleFor(x => x.CarePlanSummary).MaximumLength(500).WithMessage("Keep the care plan summary under 500 characters.");

            // A condition that calls a method: only the server can run it, so the browser does not run the rule.
            When(x => IsReturningResident(x), () =>
                RuleFor(x => x.PreviousRoom).NotEmpty().WithMessage("A returning resident's previous room is required."));

            // The server fills this in; the page has no input for it.
            RuleFor(x => x.ReviewedBy).NotEmpty().ServerOnly();
        }

        private static bool IsReturningResident(MoveInReviewModel review) => review.IsReturning;
    }

    /// <summary>
    /// The community's wristband rule, declared in the app: the framework ships no such check. It is written
    /// with the grammar's own checks, so the server and the browser decide it the same way.
    /// </summary>
    public static class WristbandRules
    {
        /// <summary>The wristband printer reads WB- and six digits.</summary>
        public static IRuleBuilderOptions<T, string?> WristbandCode<T>(this IRuleBuilder<T, string?> rule) =>
            rule.Matches(@"^WB-\d{6}$").WithMessage("Enter the wristband code as WB- and six digits, like WB-104233.");
    }
}
