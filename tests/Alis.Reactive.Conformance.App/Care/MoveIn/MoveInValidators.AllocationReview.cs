using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal static partial class MoveInValidators
{
    static partial void ForTheAllocationReview(ReactiveFluentValidationBuilder validators) => validators.Add<AllocationRequestRules>();
}

public sealed class AllocationRequestRules : ReactiveValidator<AllocationRequest>
{
    public AllocationRequestRules()
    {
        RuleFor(asked => asked.ResidentId).NotEmpty().WithName("Resident").WithMessage("Choose a resident.");
        RuleFor(asked => asked.Location).SetValidator(new LocationRules());
        RuleFor(asked => asked.Level).IsInEnum().WithName("Care level");
    }
}
