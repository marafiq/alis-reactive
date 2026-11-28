using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal static partial class MoveInValidators
{
    static partial void ForTheReception(ReactiveFluentValidationBuilder validators) => validators.Add<ReceptionRules>();
}

internal sealed class ReceptionRules : ReactiveValidator<ReceptionPage>
{
    public ReceptionRules()
    {
        RuleFor(m => m.VisitorName).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter the visitor's name.")
            .Length(2, 100).WithMessage("Enter a visitor name between two and one hundred characters.");
        RuleFor(m => m.Copies).InclusiveBetween(1, 3).WithMessage("Choose between one and three badge copies.");
    }
}
