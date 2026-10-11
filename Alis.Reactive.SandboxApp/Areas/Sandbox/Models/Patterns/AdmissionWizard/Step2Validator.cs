using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.AdmissionWizard;

public class Step2Validator : ReactiveValidator<Step2ClinicalModel>
{
    public Step2Validator()
    {
        When(x => x.PrimaryDiagnosis == "Alzheimer's", () =>
        {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m).WithMessage("'Cognitive Score' must be greater than 0.");
        });
        When(x => x.PrimaryDiagnosis == "Parkinson's", () =>
        {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m).WithMessage("'Cognitive Score' must be greater than 0.");
        });
        When(x => x.Wanders, () =>
        {
            RuleFor(x => x.WanderFrequency).NotEmpty().WithMessage("'Wander Frequency' is required.");
        });
        When(x => x.PrimaryDiagnosis == "Heart Disease", () =>
        {
            RuleFor(x => x.SystolicBP).GreaterThan(0m).WithMessage("'Systolic BP' must be greater than 0.");
        });
        When(x => x.HasPacemaker, () =>
        {
            RuleFor(x => x.PacemakerModel).NotEmpty().WithMessage("'Pacemaker Model' is required.");
        });
        When(x => x.PrimaryDiagnosis == "Diabetes", () =>
        {
            RuleFor(x => x.DiabetesType).NotEmpty().WithMessage("'Diabetes Type' is required.");
            RuleFor(x => x.A1cLevel).GreaterThan(0m).WithMessage("'A1c Level' must be greater than 0.");
        });
        When(x => x.InsulinDependent, () =>
        {
            RuleFor(x => x.InsulinSchedule).NotEmpty().WithMessage("'Insulin Schedule' is required.");
        });
    }
}
