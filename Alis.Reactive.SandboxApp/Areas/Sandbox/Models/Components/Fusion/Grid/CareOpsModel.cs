using Alis.Reactive.FluentValidator;
using FluentValidation;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Care Operations state carries the templated admit-resident inputs.
    /// Fast filters come from chips, so no filter fields live here.
    /// </summary>
    public class CareOpsViewModel
    {
        public string? NewResidentName { get; set; }
        public string? NewWing { get; set; }
        public string? NewCareLevel { get; set; }
        public string? NewRiskLevel { get; set; }
        public decimal? NewOpenTasks { get; set; }
    }

    /// <summary>Resident care row used as both server store record and grid DTO.</summary>
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

    /// <summary>Care grid response shaped for Syncfusion custom binding: {result, count}.</summary>
    public class CareOpsResponse
    {
        public List<ResidentCareItem> Result { get; set; } = new();
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
    }

    public class CareOpsSaveRequest
    {
        public FusionGridBatchChanges<ResidentCareItem>? BatchChanges { get; set; }
    }

    /// <summary>Client and server validation for the templated admit-resident dialog.</summary>
    public class CareOpsAdmitValidator : ReactiveValidator<CareOpsViewModel>
    {
        public CareOpsAdmitValidator()
        {
            RuleFor(x => x.NewResidentName).NotEmpty().WithMessage("Resident name is required.");

            RuleFor(x => x.NewResidentName).MinimumLength(3).WithMessage("Resident name must be at least 3 characters.");

            RuleFor(x => x.NewWing).NotEmpty().WithMessage("Wing is required.");

            RuleFor(x => x.NewCareLevel).NotEmpty().WithMessage("Care level is required.");

            RuleFor(x => x.NewRiskLevel).NotEmpty().WithMessage("Risk level is required.");

            RuleFor(x => x.NewOpenTasks).NotEmpty().WithMessage("Open tasks is required.");

            RuleFor(x => x.NewOpenTasks).InclusiveBetween(0m, 12m).WithMessage("Open tasks must be between 0 and 12.");
        }
    }

    /// <summary>
    /// Client and server validation for an in-cell care row edit. The same
    /// FluentValidation rules drive EJ2-native column validation in the grid
    /// (via <c>FusionGridValidation</c>) and a full server check on save.
    /// </summary>
    public class ResidentCareItemValidator : ReactiveValidator<ResidentCareItem>
    {
        public const string CriticalNeedsAnOpenTask =
            "A critical-risk resident keeps at least one open task: the daily safety check.";

        public ResidentCareItemValidator()
        {
            RuleFor(r => r.OpenTasks).InclusiveBetween(0, 7).WithMessage("Open tasks must be between 0 and 7.");

            // A grid column's rule holds for every row, so this one, which holds only for critical-risk
            // rows, is checked when the edits are saved.
            When(r => r.RiskLevel == "Critical", () =>
                RuleFor(r => r.OpenTasks).GreaterThanOrEqualTo(1).WithMessage(CriticalNeedsAnOpenTask));
        }
    }

    /// <summary>The document events the CareOps page raises and hears.</summary>
    public static class CareOpsEvents
    {
        public static readonly AppEvent CareopsRowsRefresh = AppEvent.Named("careops-rows-refresh");
    }
}
