using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal static partial class MoveInValidators
{
    static partial void ForTheWaitlist(ReactiveFluentValidationBuilder validators) => validators.Add<WaitlistRules>();
}

internal sealed class WaitlistRules : ReactiveValidator<WaitlistPage>
{
    public WaitlistRules()
    {
        RuleFor(entry => entry.Reason).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter a reason for joining the waitlist.")
            .MinimumLength(10).WithMessage("Give a reason of at least ten characters.");
        RuleFor(entry => entry.AcceptTerms).Equal(true).WithMessage("Accept the waitlist terms.");
    }
}
