using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FallRiskScreeningValidator : ReactiveValidator<FallRiskScreeningModel>
    {
        public FallRiskScreeningValidator()
        {
            RuleFor(x => x.FellInLast90Days).NotEmpty().WithMessage("Answer whether the resident fell in the last 90 days.");
            RuleFor(x => x.ObservedWalkingInPerson).NotEmpty().WithMessage("Confirm you observed the resident walking in person.");
        }
    }
}
