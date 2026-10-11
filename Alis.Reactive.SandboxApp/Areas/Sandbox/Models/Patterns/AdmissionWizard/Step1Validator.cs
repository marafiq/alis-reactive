using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.AdmissionWizard;

public class Step1Validator : ReactiveValidator<Step1DemographicsModel>
{
    public Step1Validator()
    {
        RuleFor(x => x.ResidentName).NotEmpty().WithMessage("'Resident Name' is required.");
        RuleFor(x => x.Age).GreaterThan(0m).WithMessage("'Age' must be greater than 0.");
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty().WithMessage("'Primary Diagnosis' is required.");
        When(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VaId).NotEmpty().WithMessage("'VA ID' is required.");
        });
    }
}
