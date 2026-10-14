using System.Text.Json;
using Alis.Reactive.FluentValidator;
using Microsoft.AspNetCore.Http;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper
{
    /// <summary>
    /// A new resident's admission at Maple Grove, taken in four steps: intake, care plan, contacts and review.
    /// The page loads one step at a time from the server; the admission's id keys every step it saves.
    /// </summary>
    public sealed record AdmissionWizardModel
    {
        public string AdmissionId { get; init; } = "";
    }

    /// <summary>Asks for one step of an admission, by its index on the stepper (0 is the intake).</summary>
    public sealed record AdmissionStepRequest
    {
        public string AdmissionId { get; init; } = "";
        public int Step { get; init; }
    }

    /// <summary>The intake: who is moving in, and how.</summary>
    public sealed record AdmissionIntake
    {
        public string AdmissionId { get; init; } = "";
        public string? ResidentName { get; init; }
        public decimal? Age { get; init; }
        public string? AdmissionType { get; init; }
    }

    /// <summary>The care plan the resident moves in with.</summary>
    public sealed record AdmissionCare
    {
        public string AdmissionId { get; init; } = "";

        /// <summary>The resident the intake named; the step shows it and does not post it.</summary>
        public string ResidentName { get; init; } = "";

        public string? CareLevel { get; init; }
        public string? PrimaryDiagnosis { get; init; }
        public decimal? FallRiskScore { get; init; }
        public string? MemoryAssessment { get; init; }
    }

    /// <summary>The family member responsible for the resident.</summary>
    public sealed record AdmissionContacts
    {
        public string AdmissionId { get; init; } = "";
        public string? ResponsibleParty { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
    }

    /// <summary>The whole admission as the coordinator reviews it, and who signs it off.</summary>
    public sealed record AdmissionReview
    {
        public string AdmissionId { get; init; } = "";
        public string ResidentName { get; init; } = "";
        public string AdmissionType { get; init; } = "";
        public string CareLevel { get; init; } = "";
        public string PrimaryDiagnosis { get; init; } = "";
        public string ResponsibleParty { get; init; } = "";
        public string Phone { get; init; } = "";
        public string? AdmissionCoordinator { get; init; }
    }

    /// <summary>What the server says when it saves a step or submits the admission.</summary>
    public sealed record AdmissionStepSaved
    {
        public string Message { get; init; } = "";
    }

    /// <summary>Who already lives at Maple Grove, and who signs admissions off.</summary>
    public static class MapleGroveAdmissions
    {
        public static readonly IReadOnlyList<string> CurrentResidents = ["Margaret Hale", "Arthur Bell", "Helen Park"];

        public static readonly IReadOnlyList<string> AdmissionsTeam = ["Nora Jensen", "Luis Ortega"];

        /// <summary>Maple Grove has no Skilled Nursing bed open this month.</summary>
        public const string FullCareLevel = "Skilled Nursing";
    }

    /// <summary>
    /// The admissions this browser has started: each step's last saved form, kept in the browser's session and
    /// keyed by the admission, so no other browser sees them.
    /// </summary>
    public sealed class AdmissionDrafts
    {
        private readonly ISession _session;

        public AdmissionDrafts(ISession session)
        {
            _session = session;
        }

        /// <summary>The step as last saved, or an empty one.</summary>
        public T Load<T>(string admissionId) where T : new() =>
            _session.GetString(Key<T>(admissionId)) is { } json ? JsonSerializer.Deserialize<T>(json) ?? new T() : new T();

        public void Save<T>(string admissionId, T step) =>
            _session.SetString(Key<T>(admissionId), JsonSerializer.Serialize(step));

        private static string Key<T>(string admissionId) => $"admission:{admissionId}:{typeof(T).Name}";
    }

    public sealed class AdmissionIntakeValidator : ReactiveValidator<AdmissionIntake>
    {
        public AdmissionIntakeValidator()
        {
            RuleFor(x => x.ResidentName)
                .NotEmpty().WithMessage("Enter the resident's full name.")
                .MinimumLength(2).WithMessage("Enter the resident's full name.");

            // Only the server knows who lives here now.
            RuleFor(x => x.ResidentName)
                .Must(name => !MapleGroveAdmissions.CurrentResidents.Contains(name))
                .WithMessage(intake => $"{intake.ResidentName} already lives at Maple Grove.");

            RuleFor(x => x.Age)
                .NotEmpty().WithMessage("Enter the resident's age.")
                .GreaterThanOrEqualTo(55m).WithMessage("Maple Grove admits residents aged 55 and over.")
                .LessThanOrEqualTo(120m).WithMessage("Check the resident's age.");

            RuleFor(x => x.AdmissionType).NotEmpty().WithMessage("Choose the admission type.");
        }
    }

    public sealed class AdmissionCareValidator : ReactiveValidator<AdmissionCare>
    {
        public AdmissionCareValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Choose the care level.");

            // Bed availability changes daily; only the server knows it.
            RuleFor(x => x.CareLevel)
                .Must(level => level != MapleGroveAdmissions.FullCareLevel)
                .WithMessage("No Skilled Nursing bed is open at Maple Grove. Choose Assisted Living or Memory Care.");

            RuleFor(x => x.PrimaryDiagnosis).NotEmpty().WithMessage("Choose the primary diagnosis.");

            RuleFor(x => x.FallRiskScore)
                .NotEmpty().WithMessage("Score the resident's fall risk.")
                .InclusiveBetween(0m, 10m).WithMessage("Fall risk is scored 0 to 10.");

            When(x => x.CareLevel == "Memory Care", () =>
                RuleFor(x => x.MemoryAssessment).NotEmpty().WithMessage("Write the memory assessment for a Memory Care admission."));
        }
    }

    public sealed class AdmissionContactsValidator : ReactiveValidator<AdmissionContacts>
    {
        public AdmissionContactsValidator()
        {
            RuleFor(x => x.ResponsibleParty)
                .NotEmpty().WithMessage("Name the resident's responsible party.")
                .MinimumLength(2).WithMessage("Name the resident's responsible party.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Enter the responsible party's phone.")
                .Matches(@"^\d{3}-\d{3}-\d{4}$").WithMessage("Enter the phone as 555-142-0188.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Enter the responsible party's email.")
                .EmailAddress().WithMessage("Enter an email address the family reads.");
        }
    }

    public sealed class AdmissionReviewValidator : ReactiveValidator<AdmissionReview>
    {
        public AdmissionReviewValidator()
        {
            RuleFor(x => x.AdmissionCoordinator).NotEmpty().WithMessage("Name the coordinator who signs off the admission.");

            // The admissions team is the server's to know.
            RuleFor(x => x.AdmissionCoordinator)
                .Must(name => name == null || MapleGroveAdmissions.AdmissionsTeam.Contains(name))
                .WithMessage(review => $"{review.AdmissionCoordinator} is not on Maple Grove's admissions team.");
        }
    }
}
