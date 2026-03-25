using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step4Validator : ReactiveValidator<Step4ReviewModel>
{
    public Step4Validator()
    {
        RuleFor(x => x.ScreeningId).NotEmpty().WithMessage("Please complete all steps before submitting.");
        RuleFor(x => x.EmergencyContact).NotEmpty();
    }
}
