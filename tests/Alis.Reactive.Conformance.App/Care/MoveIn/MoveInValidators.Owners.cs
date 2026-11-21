using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal static partial class MoveInValidators
{
    static partial void ForTheOwners(ReactiveFluentValidationBuilder validators) => validators.Add<MobilityRules>().Add<RoomReviewRules>();
}

internal sealed class MobilityRules : ReactiveValidator<MobilityRequest>
{
    public MobilityRules()
    {
        RuleFor(sent => sent.Ratio).InclusiveBetween(0, 100).WithMessage("The assessment ratio must be between 0 and 100.");
    }
}

internal sealed class RoomReviewRules : ReactiveValidator<RoomReviewRequest>
{
    public RoomReviewRules()
    {
        RuleFor(sent => sent.Room).Cascade(CascadeMode.Stop).NotEmpty().Matches("^[0-9]+$").WithMessage("Choose a numbered room.");
    }
}
