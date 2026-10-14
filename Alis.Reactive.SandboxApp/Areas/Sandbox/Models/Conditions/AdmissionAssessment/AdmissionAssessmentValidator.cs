using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class AdmissionAssessmentValidator : ReactiveValidator<HealthScreeningModel>
{
    public AdmissionAssessmentValidator()
    {
        RuleFor(x => x.ResidentName).NotEmpty().WithMessage("'Resident Name' is required.");
        RuleFor(x => x.Age).GreaterThan(0m).WithMessage("'Age' must be greater than 0.");
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty().WithMessage("'Primary Diagnosis' is required.");
        RuleFor(x => x.EmergencyContact).NotEmpty().WithMessage("'Emergency Contact' is required.");

        When(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VaId).NotEmpty().WithMessage("'VA ID' is required.");
        });

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
