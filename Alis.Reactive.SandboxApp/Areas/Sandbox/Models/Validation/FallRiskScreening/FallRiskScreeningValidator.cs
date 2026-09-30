using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FallRiskScreeningValidator : ReactiveValidator<FallRiskScreeningModel>
    {
        public FallRiskScreeningValidator()
        {
            ClientRule(x => x.FellInLast90Days).Required("Answer whether the resident fell in the last 90 days.");
            ClientRule(x => x.ObservedWalkingInPerson).Required("Confirm you observed the resident walking in person.");
        }
    }
}
