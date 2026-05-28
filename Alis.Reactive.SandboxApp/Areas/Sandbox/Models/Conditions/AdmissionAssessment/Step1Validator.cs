using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class Step1Validator : ReactiveValidator<Step1DemographicsModel>
{
    public Step1Validator()
    {
        RuleFor(x => x.ResidentName).NotEmpty();
        ClientRule(x => x.ResidentName).Required("'Resident Name' is required.");
        RuleFor(x => x.Age).GreaterThan(0m);
        ClientRule(x => x.Age).GreaterThan(0m, "'Age' must be greater than 0.");
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        ClientRule(x => x.PrimaryDiagnosis).Required("'Primary Diagnosis' is required.");
        WhenField(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VaId).NotEmpty();
            ClientRule(x => x.VaId).Required("'VA ID' is required.");
        });
    }
}
