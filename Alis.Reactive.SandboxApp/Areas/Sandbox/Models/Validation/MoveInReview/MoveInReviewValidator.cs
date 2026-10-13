using Alis.Reactive.FluentValidator;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

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

            // RegexOptions travel with the pattern: "ma-214" and "MA-214" are the same room.
            RuleFor(x => x.AssignedRoom)
                .Matches(@"^[a-z]{2}-\d{3}$", RegexOptions.IgnoreCase).WithMessage("Enter the room as wing and number, like MA-214.");

            Unless(x => string.IsNullOrEmpty(x.FamilyPortalLink), () =>
                RuleFor(x => x.FamilyPortalLink).Url().WithMessage("The family portal link must be a web address."));

            // The community's own validator: it names the browser check that decides what it decides.
            RuleFor(x => x.WristbandCode).WristbandCode();

            // Rules for a nested object, written inline.
            RuleFor(x => x.Pharmacy).ChildRules(pharmacy =>
                pharmacy.RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{4}$").WithMessage("Enter the pharmacy phone as 555-0100."));

            // \z is .NET's end of input; JavaScript reads it as a letter z, so the server checks this pattern.
            RuleFor(x => x.ForwardingZip)
                .Matches(@"^\d{5}\z").WithMessage("Enter the forwarding ZIP as five digits.");

            // A field another page edits: this page sends no summary, which is under 500 characters.
            RuleFor(x => x.CarePlanSummary).MaximumLength(500).WithMessage("Keep the care plan summary under 500 characters.");

            // A condition written as a method: the server decides it, so the browser does not run the rule.
            When(IsReturningResident, () =>
                RuleFor(x => x.PreviousRoom).NotEmpty().WithMessage("A returning resident's previous room is required."));

            // The server fills this in; the page has no input for it.
            RuleFor(x => x.ReviewedBy).NotEmpty().ServerOnly();
        }

        private static bool IsReturningResident(MoveInReviewModel review) => review.IsReturning;
    }

    /// <summary>The community's wristband rule, declared in the app: the framework ships no such check.</summary>
    public static class WristbandRules
    {
        public static IRuleBuilderOptions<T, string?> WristbandCode<T>(this IRuleBuilder<T, string?> rule) =>
            rule.SetValidator(new WristbandCodeValidator<T>());
    }

    /// <summary>The wristband printer reads WB- and six digits; the browser checks the same pattern.</summary>
    public sealed class WristbandCodeValidator<T> : PropertyValidator<T, string?>, IBrowserRule
    {
        private const string Code = @"^WB-\d{6}$";

        public override string Name => "WristbandCodeValidator";

        public BrowserRule Browser => BrowserRule.Pattern(Code);

        public override bool IsValid(ValidationContext<T> context, string? value) =>
            string.IsNullOrEmpty(value) || Regex.IsMatch(value, Code);

        protected override string GetDefaultMessageTemplate(string errorCode) =>
            "Enter the wristband code as WB- and six digits, like WB-104233.";
    }
}
