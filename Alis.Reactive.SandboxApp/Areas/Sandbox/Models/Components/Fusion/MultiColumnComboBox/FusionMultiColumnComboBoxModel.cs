
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiColumnComboBox
{
    public sealed class FusionMultiColumnComboBoxModel
    {
        public string? OpenShift { get; set; }

        public string? CoveringNurse { get; set; }

        public bool IncludeAgency { get; set; }

        public IReadOnlyList<OpenShiftOption> OpenShifts { get; init; } = [];

        public IReadOnlyList<RosterNurse> Roster { get; init; } = [];
    }

    public sealed class OpenShiftOption
    {
        public string Code { get; init; } = "";

        public string Label { get; init; } = "";
    }

    public sealed class RosterNurse
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";

        public string HomeWing { get; init; } = "";

        public string Shift { get; init; } = "";

        public string Certifications { get; init; } = "";

        public string Employer { get; init; } = "";
    }

    public sealed class ShiftStatusQuery
    {
        public string? Shift { get; set; }
    }

    public sealed class ShiftStanding
    {
        public string Volunteer { get; init; } = "";

        public string CoveredBy { get; init; } = "";
    }

    public sealed class RosterQuery
    {
        public bool IncludeAgency { get; set; }
    }

    public sealed class CoverageRoster
    {
        public IReadOnlyList<RosterNurse> Nurses { get; init; } = [];
    }

    public sealed class NurseHoursQuery
    {
        public string? Nurse { get; set; }
    }

    public sealed class NurseWeek
    {
        public string Summary { get; init; } = "";
    }

    public sealed class CoverageConfirmation
    {
        public string Message { get; init; } = "";
    }

    public sealed class ShiftCoverageValidator : ReactiveValidator<FusionMultiColumnComboBoxModel>
    {
        public ShiftCoverageValidator()
        {
            RuleFor(x => x.OpenShift).NotEmpty().WithMessage("Choose the open shift to cover.");
            RuleFor(x => x.CoveringNurse).NotEmpty().WithMessage("Choose the nurse who covers the shift.");
        }
    }
}
