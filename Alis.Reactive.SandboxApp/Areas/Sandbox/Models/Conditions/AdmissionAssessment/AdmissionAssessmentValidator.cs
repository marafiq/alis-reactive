using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class AdmissionAssessmentValidator : ReactiveValidator<HealthScreeningModel>
{
    public AdmissionAssessmentValidator()
    {
        RuleFor(x => x.ResidentName).NotEmpty();
        ClientRule(x => x.ResidentName).Required("'Resident Name' is required.");
        RuleFor(x => x.Age).GreaterThan(0m);
        ClientRule(x => x.Age).GreaterThan(0m, "'Age' must be greater than 0.");
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        ClientRule(x => x.PrimaryDiagnosis).Required("'Primary Diagnosis' is required.");
        RuleFor(x => x.EmergencyContact).NotEmpty();
        ClientRule(x => x.EmergencyContact).Required("'Emergency Contact' is required.");

        WhenField(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VaId).NotEmpty();
            ClientRule(x => x.VaId).Required("'VA ID' is required.");
        });

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

        WhenField(x => x.CausedInjury, () =>
        {
            RuleFor(x => x.InjuryType).NotEmpty();
            ClientRule(x => x.InjuryType).Required("'Injury Type' is required.");
        });

        WhenField(x => x.TakesPainMedication, () =>
        {
            RuleFor(x => x.PainLevel).GreaterThan(0m);
            ClientRule(x => x.PainLevel).GreaterThan(0m, "'Pain Level' must be greater than 0.");
        });
        WhenFields(fields => fields
            .Field(x => x.TakesPainMedication).Truthy()
            .And(fields.Field(x => x.PainLevel).Gt(7m)), () =>
            {
                RuleFor(x => x.PainLocation)
                    .NotEmpty()
                    .WithMessage("'Pain Location' is required for severe pain.");
                ClientRule(x => x.PainLocation)
                    .Required("'Pain Location' is required for severe pain.");
            });
    }
}
