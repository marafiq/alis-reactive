using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step3Validator : ReactiveValidator<Step3FunctionalModel>
{
    public Step3Validator()
    {
        WhenField(x => x.CausedInjury, () => { RuleFor(x => x.InjuryType).NotEmpty(); });
        WhenField(x => x.TakesPainMedication, () => { RuleFor(x => x.PainLevel).GreaterThan(0m); });
    }
}
