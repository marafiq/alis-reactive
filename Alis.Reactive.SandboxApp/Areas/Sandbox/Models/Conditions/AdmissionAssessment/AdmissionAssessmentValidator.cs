using FluentValidation;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class AdmissionAssessmentValidator : ReactiveValidator<HealthScreeningModel>
{
    public AdmissionAssessmentValidator()
    {
        // Always required
        RuleFor(x => x.ResidentName).NotEmpty();
        RuleFor(x => x.Age).GreaterThan(0m);
        RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
        RuleFor(x => x.EmergencyContact).NotEmpty();

        // Veteran conditional
        WhenField(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VaId).NotEmpty();
        });

        // Cognitive conditional (Alzheimer's)
        WhenField(x => x.PrimaryDiagnosis, "Alzheimer's", () =>
        {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m);
        });

        // Cognitive conditional (Parkinson's)
        WhenField(x => x.PrimaryDiagnosis, "Parkinson's", () =>
        {
            RuleFor(x => x.CognitiveScore).GreaterThan(0m);
        });

        // Wandering conditional
        WhenField(x => x.Wanders, () =>
        {
            RuleFor(x => x.WanderFrequency).NotEmpty();
        });

        // Cardiac conditional
        WhenField(x => x.PrimaryDiagnosis, "Heart Disease", () =>
        {
            RuleFor(x => x.SystolicBP).GreaterThan(0m);
        });

        // Pacemaker conditional
        WhenField(x => x.HasPacemaker, () =>
        {
            RuleFor(x => x.PacemakerModel).NotEmpty();
        });

        // Diabetes conditional
        WhenField(x => x.PrimaryDiagnosis, "Diabetes", () =>
        {
            RuleFor(x => x.DiabetesType).NotEmpty();
            RuleFor(x => x.A1cLevel).GreaterThan(0m);
        });

        // Insulin conditional
        WhenField(x => x.InsulinDependent, () =>
        {
            RuleFor(x => x.InsulinSchedule).NotEmpty();
        });

        // Falls conditional
        WhenField(x => x.CausedInjury, () =>
        {
            RuleFor(x => x.InjuryType).NotEmpty();
        });

        // Pain conditional
        WhenField(x => x.TakesPainMedication, () =>
        {
            RuleFor(x => x.PainLevel).GreaterThan(0m);
        });

        // NOTE: PainLocation required when PainLevel > 7 is handled server-side only.
        // WhenField does not support numeric thresholds (only truthy/eq/neq).
        // The conditions DSL shows the "required" indicator via When(comp.Value()).Gt(7m).
    }
}
