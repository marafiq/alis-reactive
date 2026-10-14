using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiColumnComboBox
{
    /// <summary>
    /// A staffing coordinator covers an open nursing shift: she picks the covering nurse from a list showing
    /// each nurse's home wing, usual shift and certifications, with agency nurses added when she asks.
    /// </summary>
    public sealed class FusionMultiColumnComboBoxModel
    {
        /// <summary>The open shift being covered, by its code.</summary>
        public string? OpenShift { get; set; }

        /// <summary>The nurse who covers it, by nurse id.</summary>
        public string? CoveringNurse { get; set; }

        /// <summary>Whether agency nurses are offered beside the community's own.</summary>
        public bool IncludeAgency { get; set; }

        /// <summary>The open shifts this week.</summary>
        public IReadOnlyList<OpenShiftOption> OpenShifts { get; init; } = [];

        /// <summary>The community's own nurses.</summary>
        public IReadOnlyList<RosterNurse> Roster { get; init; } = [];
    }

    /// <summary>An open shift: the code staffing keeps and the line the coordinator reads.</summary>
    public sealed class OpenShiftOption
    {
        public string Code { get; init; } = "";

        public string Label { get; init; } = "";
    }

    /// <summary>A nurse the coordinator can call in, as the nurse list shows her.</summary>
    public sealed class RosterNurse
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";

        public string HomeWing { get; init; } = "";

        public string Shift { get; init; } = "";

        public string Certifications { get; init; } = "";

        /// <summary>Maple Grove staff, or the agency that sends her.</summary>
        public string Employer { get; init; } = "";
    }

    /// <summary>Asks how an open shift stands with the staffing line.</summary>
    public sealed class ShiftStatusQuery
    {
        public string? Shift { get; set; }
    }

    /// <summary>
    /// How an open shift stands with the staffing line; each member is empty when there is none.
    /// </summary>
    public sealed class ShiftStanding
    {
        /// <summary>The nurse who volunteered for the shift by text, by nurse id.</summary>
        public string Volunteer { get; init; } = "";

        /// <summary>The nurse who already covers the shift, by name.</summary>
        public string CoveredBy { get; init; } = "";
    }

    /// <summary>Asks for the nurses the coordinator can call in.</summary>
    public sealed class RosterQuery
    {
        public bool IncludeAgency { get; set; }
    }

    /// <summary>The nurses the coordinator can call in.</summary>
    public sealed class CoverageRoster
    {
        public IReadOnlyList<RosterNurse> Nurses { get; init; } = [];
    }

    /// <summary>Asks for the hours a nurse has worked this week.</summary>
    public sealed class NurseHoursQuery
    {
        public string? Nurse { get; set; }
    }

    /// <summary>The hours a nurse has worked this week, as the coordinator reads them.</summary>
    public sealed class NurseWeek
    {
        public string Summary { get; init; } = "";
    }

    /// <summary>What staffing says once the shift is assigned, or why it refused the nurse.</summary>
    public sealed class CoverageConfirmation
    {
        public string Message { get; init; } = "";
    }

    /// <summary>A shift is assigned only once the shift and the nurse who covers it are chosen.</summary>
    public sealed class ShiftCoverageValidator : ReactiveValidator<FusionMultiColumnComboBoxModel>
    {
        public ShiftCoverageValidator()
        {
            RuleFor(x => x.OpenShift).NotEmpty().WithMessage("Choose the open shift to cover.");
            RuleFor(x => x.CoveringNurse).NotEmpty().WithMessage("Choose the nurse who covers the shift.");
        }
    }
}
