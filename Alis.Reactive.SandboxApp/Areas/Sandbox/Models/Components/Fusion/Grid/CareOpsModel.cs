using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CareOpsViewModel
    {
        public string? FilterRisk { get; set; }

        public string? FilterCareLevel { get; set; }

        public string? NewResidentName { get; set; }
        public string? NewWing { get; set; }
        public string? NewCareLevel { get; set; }
        public string? NewRiskLevel { get; set; }
        public decimal? NewOpenTasks { get; set; }
    }

    public static class CareOpsOptions
    {
        public static readonly IReadOnlyList<string> Wings = new[] { "North", "East", "West", "South" };
        public static readonly IReadOnlyList<string> CareLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
        public static readonly IReadOnlyList<string> RiskLevels = new[] { "Low", "Moderate", "High", "Critical" };
        public static readonly IReadOnlyList<string> Nurses = new[] { "Nora Ellis", "Malik Stone", "Elena Ruiz", "Owen Park", "Night Float Team" };
    }

    public class ResidentCareItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Wing { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public int OpenTasks { get; set; }

        public string NextReview { get; set; } = "";
    }

    public class CareOpsResponse : IFusionGridPage<ResidentCareItem>
    {
        public List<ResidentCareItem> Result { get; set; } = new();

        IReadOnlyList<ResidentCareItem> IFusionGridPage<ResidentCareItem>.Result => Result;
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    public class CareOpsDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<FusionGridSortColumn>? Sorted { get; set; }
        public string? FilterRisk { get; set; }
        public string? FilterCareLevel { get; set; }
    }

    public class CareOpsBulkRequest
    {
        public List<ResidentCareItem>? SelectedRecords { get; set; }
        public string? Nurse { get; set; }
        public string? Risk { get; set; }
        public string? FilterRisk { get; set; }
        public string? FilterCareLevel { get; set; }
    }

    public class CareOpsSaveRequest
    {
        public FusionGridBatchChanges<ResidentCareItem>? BatchChanges { get; set; }
        public string? FilterRisk { get; set; }
        public string? FilterCareLevel { get; set; }
    }

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

    public class ResidentCareItemValidator : ReactiveValidator<ResidentCareItem>
    {
        public const string CriticalNeedsAnOpenTask =
            "A critical-risk resident keeps at least one open task: the daily safety check.";

        public ResidentCareItemValidator()
        {
            RuleFor(r => r.OpenTasks).InclusiveBetween(0, 7).WithMessage("Open tasks run from 0 to 7.");

            When(r => r.RiskLevel == "Critical", () =>
                RuleFor(r => r.OpenTasks).GreaterThanOrEqualTo(1).WithMessage(CriticalNeedsAnOpenTask));
        }
    }

    public static class CareOpsEvents
    {
        public static readonly AppEvent CareopsRowsRefresh = AppEvent.Declare();
    }
}
