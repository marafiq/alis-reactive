using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The care operations board: the risk and care level its chips chose, which every request for the board's
    /// rows sends, and the admit dialog's fields.
    /// </summary>
    public class CareOpsViewModel
    {
        /// <summary>The risk the risk chips chose; empty or "All" lists every risk.</summary>
        public string? FilterRisk { get; set; }

        /// <summary>The care level the care-level chips chose; empty or "All" lists every care level.</summary>
        public string? FilterCareLevel { get; set; }

        public string? NewResidentName { get; set; }
        public string? NewWing { get; set; }
        public string? NewCareLevel { get; set; }
        public string? NewRiskLevel { get; set; }
        public decimal? NewOpenTasks { get; set; }
    }

    /// <summary>The choices the board's chips, cell editors and admit dialog offer.</summary>
    public static class CareOpsOptions
    {
        public static readonly IReadOnlyList<string> Wings = new[] { "North", "East", "West", "South" };
        public static readonly IReadOnlyList<string> CareLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
        public static readonly IReadOnlyList<string> RiskLevels = new[] { "Low", "Moderate", "High", "Critical" };
        public static readonly IReadOnlyList<string> Nurses = new[] { "Nora Ellis", "Malik Stone", "Elena Ruiz", "Owen Park", "Night Float Team" };
    }

    /// <summary>A resident's care on the board: risk, primary nurse, open care tasks and next review.</summary>
    public class ResidentCareItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Wing { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public int OpenTasks { get; set; }

        /// <summary>The day of the next care review, as the date editor writes it: <c>2026-10-16</c>.</summary>
        public string NextReview { get; set; } = "";
    }

    /// <summary>
    /// The board's first page or the page the grid asked for (<c>{result, count}</c>), and the sentence that
    /// says what the server did.
    /// </summary>
    public class CareOpsResponse
    {
        public List<ResidentCareItem> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>The page and sort the grid asks for, and the fast filters the chips chose.</summary>
    public class CareOpsDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<FusionGridSortColumn>? Sorted { get; set; }
        public string? FilterRisk { get; set; }
        public string? FilterCareLevel { get; set; }
    }

    /// <summary>The residents ticked on the board, and the nurse or risk they take.</summary>
    public class CareOpsBulkRequest
    {
        public List<ResidentCareItem>? SelectedRecords { get; set; }
        public string? Nurse { get; set; }
        public string? Risk { get; set; }
    }

    /// <summary>The care levels, risks, nurses, tasks and review days changed in the grid.</summary>
    public class CareOpsSaveRequest
    {
        public FusionGridBatchChanges<ResidentCareItem>? BatchChanges { get; set; }
    }

    /// <summary>The admit dialog's rules.</summary>
    public class CareOpsAdmitValidator : ReactiveValidator<CareOpsViewModel>
    {
        public CareOpsAdmitValidator()
        {
            RuleFor(x => x.NewResidentName)
                .NotEmpty().WithMessage("Enter the resident's name.")
                .MinimumLength(3).WithMessage("Write the resident's full name.");

            RuleFor(x => x.NewWing).NotEmpty().WithMessage("Choose the resident's wing.");

            RuleFor(x => x.NewCareLevel).NotEmpty().WithMessage("Choose the resident's care level.");

            RuleFor(x => x.NewRiskLevel).NotEmpty().WithMessage("Choose the resident's risk.");

            RuleFor(x => x.NewOpenTasks)
                .NotNull().WithMessage("Enter the resident's open care tasks, or 0.")
                .InclusiveBetween(0m, 7m).WithMessage("Open tasks run from 0 to 7.");
        }
    }

    /// <summary>
    /// The rules for a resident's care row. The grid's open-tasks cell checks the range as the server does
    /// when the edits are saved; the server alone checks the rule that holds only for critical-risk rows.
    /// </summary>
    public class ResidentCareItemValidator : ReactiveValidator<ResidentCareItem>
    {
        public const string CriticalNeedsAnOpenTask =
            "A critical-risk resident keeps at least one open task: the daily safety check.";

        public ResidentCareItemValidator()
        {
            RuleFor(r => r.OpenTasks).InclusiveBetween(0, 7).WithMessage("Open tasks run from 0 to 7.");

            // A grid column's rule holds for every row, so this one, which holds only for critical-risk
            // rows, is checked when the edits are saved.
            When(r => r.RiskLevel == "Critical", () =>
                RuleFor(r => r.OpenTasks).GreaterThanOrEqualTo(1).WithMessage(CriticalNeedsAnOpenTask));
        }
    }

    /// <summary>The document events the CareOps page raises and hears.</summary>
    public static class CareOpsEvents
    {
        /// <summary>A resident on the high-risk watch list was flagged or discharged.</summary>
        public static readonly AppEvent CareopsRowsRefresh = AppEvent.Named("careops-rows-refresh");
    }
}
