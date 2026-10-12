using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class GridModel
    {
        public decimal? MinAge { get; set; }
    }

    public class ResidentGridItem
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    /// <summary>
    /// Server-side grid response. Syncfusion Grid custom binding expects {result, count}.
    /// Set as dataSource directly: ej2.dataSource = {result: [...], count: N}.
    /// </summary>
    public class ResidentGridResponse
    {
        public List<ResidentGridItem> Result { get; set; } = new();
        public int Count { get; set; }
    }

    public class ResidentDirectoryModel
    {
        public string? ResidentSearch { get; set; }
        public string? CareLevel { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? MinimumAge { get; set; }
    }

    public class ResidentDirectoryGridItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public int Age { get; set; }
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public int OpenTasks { get; set; }
        public string NextReviewDate { get; set; } = "";

        public string? Key { get; set; }
        public int Count { get; set; }
        public List<ResidentDirectoryGridItem>? Items { get; set; }
        public string? Field { get; set; }
    }

    public class ResidentDirectoryResponse
    {
        public List<ResidentDirectoryGridItem> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>Nested envelope so the grid can bind from a nested data-source property path.</summary>
    public class ResidentRosterEnvelope
    {
        public ResidentDirectoryResponse Page { get; set; } = new();
    }

    public class ResidentDirectorySelectionResponse
    {
        public string Summary { get; set; } = "";
        public string ResidentName { get; set; } = "";
    }

    public class ResidentGridEditingModel
    {
        public string? ResidentName { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? OpenTasks { get; set; }
    }

    public class GridOperationsModel
    {
        public string? PatchRiskLevel { get; set; }
        public decimal? PatchOpenTasks { get; set; }
    }

    public class ResidentGridEditResponse
    {
        public ResidentDirectoryGridItem Row { get; set; } = new ResidentDirectoryGridItem();
        public string Summary { get; set; } = "";
    }

    public class ResidentGridOperationsResponse
    {
        public ResidentDirectoryGridItem Row { get; set; } = new ResidentDirectoryGridItem();
        public string Summary { get; set; } = "";
    }

    public class ResidentGridBatchSummaryResponse
    {
        public string Summary { get; set; } = "";
    }

    public class GridTemplateActionPayload
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// The body <c>POST Grid/Data</c> binds: the page, the sort and the age filter. The page also sends the
    /// details of the grid's dataStateChange event beside them, which the endpoint does not read.
    /// </summary>
    public class GridDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<FusionGridSortColumn>? Sorted { get; set; }
        public decimal? MinAge { get; set; }

        public string? EventName { get; set; }
        public bool RequiresCounts { get; set; }
        public string? ActionName { get; set; }
        public string? ActionType { get; set; }
        public bool ActionCancel { get; set; }
        public string? ActionRequestType { get; set; }
        public int ActionCurrentPage { get; set; }
        public int ActionPreviousPage { get; set; }
        public int ActionPageSize { get; set; }
    }

    /// <summary>The body <c>POST Grid/DirectoryData</c> binds: the grid's data state and the directory filters.</summary>
    public class ResidentDirectoryRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<FusionGridSortColumn>? Sorted { get; set; }
        public List<string>? Group { get; set; }
        public List<FusionGridTextFilterCriterion>? Where { get; set; }
        public List<FusionGridSearchDescriptor>? Search { get; set; }
        public string? ResidentSearch { get; set; }
        public string? CareLevel { get; set; }
        public string? RiskLevel { get; set; }
        public decimal? MinimumAge { get; set; }
    }

    /// <summary>The body <c>POST Grid/SelectResident</c> binds: the selected row's resident and index.</summary>
    public class ResidentSelectionRequest
    {
        public int ResidentId { get; set; }
        public int RowIndex { get; set; }
    }

    /// <summary>The body <c>POST Grid/SelectionIndexes</c> binds.</summary>
    public class ResidentSelectionIndexesRequest
    {
        public List<int>? SelectedRowIndexes { get; set; }
    }

    /// <summary>The body <c>POST Grid/SelectionRecords</c> binds.</summary>
    public class ResidentSelectionRecordsRequest
    {
        public List<ResidentDirectoryGridItem>? SelectedRecords { get; set; }
    }

    /// <summary>The body <c>POST Grid/CurrentViewSummary</c> binds.</summary>
    public class ResidentCurrentViewRequest
    {
        public List<ResidentDirectoryGridItem>? Records { get; set; }
    }

    /// <summary>The body <c>POST Grid/RowIndexSummary</c> binds.</summary>
    public class ResidentRowIndexRequest
    {
        public int RowIndex { get; set; }
    }

    /// <summary>The body <c>POST Grid/BatchSummary</c> binds.</summary>
    public class ResidentGridBatchSummaryRequest
    {
        public FusionGridBatchChanges<ResidentDirectoryGridItem>? BatchChanges { get; set; }
    }

    public class ResidentGridEditingValidator : ReactiveValidator<ResidentGridEditingModel>
    {
        public ResidentGridEditingValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");

            RuleFor(x => x.ResidentName).MinimumLength(3).WithMessage("Resident name must be at least 3 characters.");

            RuleFor(x => x.RiskLevel).NotEmpty().WithMessage("Risk level is required.");

            RuleFor(x => x.RiskLevel)
                .Must(value => value is "Low" or "Moderate" or "High")
                .WithMessage("Risk level must be Low, Moderate, or High.");

            RuleFor(x => x.OpenTasks).NotEmpty().WithMessage("Open tasks is required.");

            RuleFor(x => x.OpenTasks).InclusiveBetween(0m, 7m).WithMessage("Open tasks must be between 0 and 7.");
        }
    }

    /// <summary>The document events the GridOperations page raises and hears.</summary>
    public static class GridOperationsEvents
    {
        public static readonly AppEvent<GridTemplateActionPayload> GridReviewResident = AppEvent.Named<GridTemplateActionPayload>("grid:review-resident");
    }
}
