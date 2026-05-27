using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class Step3Validator : ReactiveValidator<Step3FunctionalModel>
{
    public Step3Validator()
    {
        WhenField(x => x.CausedInjury, () => { RuleFor(x => x.InjuryType).NotEmpty(); });
        WhenField(x => x.TakesPainMedication, () => { RuleFor(x => x.PainLevel).GreaterThan(0m); });
        WhenFields(fields => fields
            .Field(x => x.TakesPainMedication).Truthy()
            .And(fields.Field(x => x.PainLevel).Gt(7m)), () =>
            {
                RuleFor(x => x.PainLocation)
                    .NotEmpty()
                    .WithMessage("'Pain Location' is required for severe pain.");
            });
    }
}
