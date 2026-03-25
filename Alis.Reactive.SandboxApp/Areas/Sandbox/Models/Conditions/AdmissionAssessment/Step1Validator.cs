using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step1Validator : ReactiveValidator<Step1DemographicsModel>
{
    public Step1Validator()
    {
        RuleFor(x => x.ResidentName).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0m);
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        WhenField(x => x.IsVeteran, () => { RuleFor(x => x.VaId).NotEmpty(); });
    }
}
