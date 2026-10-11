using Alis.Reactive.FluentValidator;
using FluentValidation;

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
            // FluentValidation's own message, named the way FluentValidation names the field.
            RuleFor(x => x.ResidentName).NotEmpty();

            RuleFor(x => x.FallRiskScore)
                .NotNull().WithMessage("Score the resident's fall risk.")
                .InclusiveBetween(0, 10).WithMessage("Fall risk is scored 0 to 10.");

            When(x => x.FallRiskScore > 7, () =>
                    RuleFor(x => x.MobilityNotes).NotEmpty().WithMessage("Describe the mobility plan for a high fall risk."))
                .Otherwise(() =>
                    RuleFor(x => x.MobilityNotes).MaximumLength(40).WithMessage("Keep mobility notes under 40 characters."));

            Unless(x => string.IsNullOrEmpty(x.FamilyPortalLink), () =>
                RuleFor(x => x.FamilyPortalLink).Url().WithMessage("The family portal link must be a web address."));

            // Rules for a nested object, written inline.
            RuleFor(x => x.Pharmacy).ChildRules(pharmacy =>
                pharmacy.RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{4}$").WithMessage("Enter the pharmacy phone as 555-0100."));

            // A condition written as a method: the server decides it, so the browser does not run the rule.
            When(IsReturningResident, () =>
                RuleFor(x => x.PreviousRoom).NotEmpty().WithMessage("A returning resident's previous room is required."));

            // The server fills this in; the page has no input for it.
            RuleFor(x => x.ReviewedBy).NotEmpty().ServerOnly();
        }

        private static bool IsReturningResident(MoveInReviewModel review) => review.IsReturning;
    }
}
