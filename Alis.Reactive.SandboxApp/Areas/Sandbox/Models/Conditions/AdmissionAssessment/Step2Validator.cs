using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step2Validator : ReactiveValidator<Step2ClinicalModel>
{
    public Step2Validator()
    {
        WhenField(x => x.PrimaryDiagnosis, "Alzheimer's", () => { RuleFor(x => x.CognitiveScore).GreaterThan(0m); });
        WhenField(x => x.PrimaryDiagnosis, "Parkinson's", () => { RuleFor(x => x.CognitiveScore).GreaterThan(0m); });
        WhenField(x => x.Wanders, () => { RuleFor(x => x.WanderFrequency).NotEmpty(); });
        WhenField(x => x.PrimaryDiagnosis, "Heart Disease", () => { RuleFor(x => x.SystolicBP).GreaterThan(0m); });
        WhenField(x => x.HasPacemaker, () => { RuleFor(x => x.PacemakerModel).NotEmpty(); });
        WhenField(x => x.PrimaryDiagnosis, "Diabetes", () =>
        {
            RuleFor(x => x.DiabetesType).NotEmpty();
            RuleFor(x => x.A1cLevel).GreaterThan(0m);
        });
        WhenField(x => x.InsulinDependent, () => { RuleFor(x => x.InsulinSchedule).NotEmpty(); });
    }
}
