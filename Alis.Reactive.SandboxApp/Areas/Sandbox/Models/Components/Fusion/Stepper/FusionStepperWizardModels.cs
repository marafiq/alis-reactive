using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper
{
    public sealed record FusionStepperWizardShellModel
    {
        public string? WizardId { get; init; }
        public string MaxUnlockedStep { get; init; } = "0";
    }

    public sealed record FusionStepperWizardLoadStepRequest
    {
        public string? WizardId { get; init; }
        public int Step { get; init; }
    }

    public sealed record FusionStepperWizardIntakeModel
    {
        public string? WizardId { get; init; }
        public string? ResidentName { get; init; }
        public decimal? Age { get; init; }
        public string? AdmissionType { get; init; }
    }

    public sealed record FusionStepperWizardCareModel
    {
        public string? WizardId { get; init; }
        public string? ResidentName { get; init; }
        public string? CareLevel { get; init; }
        public string? PrimaryDiagnosis { get; init; }
        public string? MemoryAssessment { get; init; }
        public decimal? FallRiskScore { get; init; }
    }

    public sealed record FusionStepperWizardContactModel
    {
        public string? WizardId { get; init; }
        public string? ResponsibleParty { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
    }

    public sealed record FusionStepperWizardReviewModel
    {
        public string? WizardId { get; init; }
        public string? ResidentName { get; init; }
        public string? AdmissionType { get; init; }
        public string? CareLevel { get; init; }
        public string? PrimaryDiagnosis { get; init; }
        public string? ResponsibleParty { get; init; }
        public string? Phone { get; init; }
        public string? AdmissionCoordinator { get; init; }
    }

    public sealed record FusionStepperWizardSaveResponse
    {
        public string WizardId { get; init; } = "";
        public string Message { get; init; } = "";
    }

    public sealed record FusionStepperWizardSubmitResponse
    {
        public string WizardId { get; init; } = "";
        public string Message { get; init; } = "";
        public string CareSummary { get; init; } = "";
        public string ContactSummary { get; init; } = "";
    }

    public sealed class FusionStepperWizardIntakeValidator : ReactiveValidator<FusionStepperWizardIntakeModel>
    {
        public FusionStepperWizardIntakeValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().MinimumLength(2);
            ClientRule(x => x.ResidentName)
                .Required("'Resident Name' is required.")
                .MinLength(2, "'Resident Name' must have a minimum length of 2.");

            RuleFor(x => x.Age).NotNull().InclusiveBetween(55m, 120m);
            ClientRule(x => x.Age)
                .Required("'Age' is required.")
                .GreaterThanOrEqualTo(55m, "'Age' must be at least 55.")
                .LessThanOrEqualTo(120m, "'Age' must be at most 120.");

            RuleFor(x => x.AdmissionType).NotEmpty();
            ClientRule(x => x.AdmissionType)
                .Required("'Admission Type' is required.");
        }
    }

    public sealed class FusionStepperWizardCareValidator : ReactiveValidator<FusionStepperWizardCareModel>
    {
        public FusionStepperWizardCareValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty();
            ClientRule(x => x.CareLevel)
                .Required("'Care Level' is required.");

            RuleFor(x => x.PrimaryDiagnosis).NotEmpty();
            ClientRule(x => x.PrimaryDiagnosis)
                .Required("'Primary Diagnosis' is required.");

            RuleFor(x => x.FallRiskScore).NotNull().InclusiveBetween(0m, 10m);
            ClientRule(x => x.FallRiskScore)
                .Required("'Fall Risk Score' is required.")
                .GreaterThanOrEqualTo(0m, "'Fall Risk Score' must be at least 0.")
                .LessThanOrEqualTo(10m, "'Fall Risk Score' must be at most 10.");

            When(x => x.CareLevel == "Memory Care", () =>
            {
                RuleFor(x => x.MemoryAssessment).NotEmpty();
            });

            WhenField(x => x.CareLevel, "Memory Care", () =>
            {
                ClientRule(x => x.MemoryAssessment)
                    .Required("'Memory Assessment' is required for Memory Care.");
            });
        }
    }

    public sealed class FusionStepperWizardContactValidator : ReactiveValidator<FusionStepperWizardContactModel>
    {
        public FusionStepperWizardContactValidator()
        {
            RuleFor(x => x.ResponsibleParty).NotEmpty().MinimumLength(2);
            ClientRule(x => x.ResponsibleParty)
                .Required("'Responsible Party' is required.")
                .MinLength(2, "'Responsible Party' must have a minimum length of 2.");

            RuleFor(x => x.Phone).NotEmpty().Matches(@"^\d{3}-\d{3}-\d{4}$");
            ClientRule(x => x.Phone)
                .Required("'Phone' is required.")
                .Regex(@"^\d{3}-\d{3}-\d{4}$", "'Phone' must match 123-456-7890.");

            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            ClientRule(x => x.Email)
                .Required("'Email' is required.")
                .Email("'Email' must be a valid email address.");
        }
    }

    public sealed class FusionStepperWizardReviewValidator : ReactiveValidator<FusionStepperWizardReviewModel>
    {
        public FusionStepperWizardReviewValidator()
        {
            RuleFor(x => x.AdmissionCoordinator).NotEmpty().MinimumLength(2);
            ClientRule(x => x.AdmissionCoordinator)
                .Required("'Admission Coordinator' is required.")
                .MinLength(2, "'Admission Coordinator' must have a minimum length of 2.");
        }
    }
}
