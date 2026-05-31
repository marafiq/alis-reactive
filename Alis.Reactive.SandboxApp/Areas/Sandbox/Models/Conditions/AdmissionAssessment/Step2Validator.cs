using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class Step2Validator : ReactiveValidator<Step2ClinicalModel>
{
    public Step2Validator()
    {
        WhenField(x => x.PrimaryDiagnosis, "Alzheimer's", () =>
        {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m);
            ClientRule(x => x.CognitiveScore).GreaterThan(0m, "'Cognitive Score' must be greater than 0.");
        });
        WhenField(x => x.PrimaryDiagnosis, "Parkinson's", () =>
        {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m);
            ClientRule(x => x.CognitiveScore).GreaterThan(0m, "'Cognitive Score' must be greater than 0.");
        });
        WhenField(x => x.Wanders, () =>
        {
            RuleFor(x => x.WanderFrequency).NotEmpty();
            ClientRule(x => x.WanderFrequency).Required("'Wander Frequency' is required.");
        });
        WhenField(x => x.PrimaryDiagnosis, "Heart Disease", () =>
        {
            RuleFor(x => x.SystolicBP).GreaterThan(0m);
            ClientRule(x => x.SystolicBP).GreaterThan(0m, "'Systolic BP' must be greater than 0.");
        });
        WhenField(x => x.HasPacemaker, () =>
        {
            RuleFor(x => x.PacemakerModel).NotEmpty();
            ClientRule(x => x.PacemakerModel).Required("'Pacemaker Model' is required.");
        });
        WhenField(x => x.PrimaryDiagnosis, "Diabetes", () =>
        {
            RuleFor(x => x.DiabetesType).NotEmpty();
            RuleFor(x => x.A1cLevel).GreaterThan(0m);
            ClientRule(x => x.DiabetesType).Required("'Diabetes Type' is required.");
            ClientRule(x => x.A1cLevel).GreaterThan(0m, "'A1c Level' must be greater than 0.");
        });
        WhenField(x => x.InsulinDependent, () =>
        {
            RuleFor(x => x.InsulinSchedule).NotEmpty();
            ClientRule(x => x.InsulinSchedule).Required("'Insulin Schedule' is required.");
        });
    }
}
