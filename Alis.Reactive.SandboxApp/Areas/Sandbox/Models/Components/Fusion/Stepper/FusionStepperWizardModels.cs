using Alis.Reactive.FluentValidator;

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
            RuleFor(x => x.ResidentName)
                .NotEmpty().WithMessage("'Resident Name' is required.")
                .MinimumLength(2).WithMessage("'Resident Name' must have a minimum length of 2.");

            RuleFor(x => x.Age)
                .NotEmpty().WithMessage("'Age' is required.")
                .GreaterThanOrEqualTo(55m).WithMessage("'Age' must be at least 55.")
                .LessThanOrEqualTo(120m).WithMessage("'Age' must be at most 120.");

            RuleFor(x => x.AdmissionType).NotEmpty().WithMessage("'Admission Type' is required.");
        }
    }

    public sealed class FusionStepperWizardCareValidator : ReactiveValidator<FusionStepperWizardCareModel>
    {
        public FusionStepperWizardCareValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");

            RuleFor(x => x.PrimaryDiagnosis).NotEmpty().WithMessage("'Primary Diagnosis' is required.");

            RuleFor(x => x.FallRiskScore)
                .NotEmpty().WithMessage("'Fall Risk Score' is required.")
                .GreaterThanOrEqualTo(0m).WithMessage("'Fall Risk Score' must be at least 0.")
                .LessThanOrEqualTo(10m).WithMessage("'Fall Risk Score' must be at most 10.");

            When(x => x.CareLevel == "Memory Care", () =>
            {
                RuleFor(x => x.MemoryAssessment).NotEmpty().WithMessage("'Memory Assessment' is required for Memory Care.");
            });
        }
    }

    public sealed class FusionStepperWizardContactValidator : ReactiveValidator<FusionStepperWizardContactModel>
    {
        public FusionStepperWizardContactValidator()
        {
            RuleFor(x => x.ResponsibleParty)
                .NotEmpty().WithMessage("'Responsible Party' is required.")
                .MinimumLength(2).WithMessage("'Responsible Party' must have a minimum length of 2.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("'Phone' is required.")
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("'Phone' must match 123-456-7890.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");
        }
    }

    public sealed class FusionStepperWizardReviewValidator : ReactiveValidator<FusionStepperWizardReviewModel>
    {
        public FusionStepperWizardReviewValidator()
        {
            RuleFor(x => x.AdmissionCoordinator)
                .NotEmpty().WithMessage("'Admission Coordinator' is required.")
                .MinimumLength(2).WithMessage("'Admission Coordinator' must have a minimum length of 2.");
        }
    }
}
