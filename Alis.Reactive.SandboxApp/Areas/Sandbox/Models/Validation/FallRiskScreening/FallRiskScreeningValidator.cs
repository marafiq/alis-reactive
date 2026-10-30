using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FallRiskScreeningValidator : ReactiveValidator<FallRiskScreeningModel>
    {
        public FallRiskScreeningValidator()
        {
            RuleFor(x => x.FellInLast90Days).NotEmpty().WithMessage("Answer whether the resident fell in the last 90 days.");
            RuleFor(x => x.ObservedWalkingInPerson).NotEmpty().WithMessage("Confirm you observed the resident walking in person.");

            // A resident who has not fallen has no falls to count and no fall injury to report.
            When(x => x.FellInLast90Days == false, () =>
            {
                RuleFor(x => x.FallsCount).Empty().WithMessage("Leave the number of falls at 0 when the resident has not fallen.");
                RuleFor(x => x.InjuryReported).Empty().WithMessage("Clear the injury box when the resident has not fallen.");
            });

            // An unwitnessed fall may hide a head injury, so neuro checks start at once.
            When(x => x.FallFactors.Contains("Unwitnessed fall"), () =>
                RuleFor(x => x.NeuroChecksStartedAt).NotEmpty().WithMessage("Record when neuro checks started after the unwitnessed fall."));
        }
    }
}
