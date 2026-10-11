using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.AdmissionWizard;

public class Step3Validator : ReactiveValidator<Step3FunctionalModel>
{
    public Step3Validator()
    {
        When(x => x.CausedInjury, () =>
        {
            RuleFor(x => x.InjuryType).NotEmpty().WithMessage("'Injury Type' is required.");
        });
        When(x => x.TakesPainMedication, () =>
        {
            RuleFor(x => x.PainLevel).GreaterThan(0m).WithMessage("'Pain Level' must be greater than 0.");
        });
        When(x => x.TakesPainMedication && x.PainLevel > 7m, () =>
            {
                RuleFor(x => x.PainLocation).NotEmpty().WithMessage("'Pain Location' is required for severe pain.");
            });
    }
}
