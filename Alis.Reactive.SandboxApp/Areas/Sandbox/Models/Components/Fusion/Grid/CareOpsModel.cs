using System.Collections.Generic;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Page model for the Care Operations board. Carries the templated "Admit resident"
    /// dialog inputs. Fast filters come from chips, so no filter fields live here.
    /// </summary>
    public class CareOpsViewModel
    {
        public string? NewResidentName { get; set; }
        public string? NewWing { get; set; }
        public string? NewCareLevel { get; set; }
        public string? NewRiskLevel { get; set; }
        public decimal? NewOpenTasks { get; set; }
    }

    /// <summary>One resident's care row. Server store record and grid DTO.</summary>
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

    /// <summary>Server-side care response. Syncfusion Grid custom binding expects {result, count}.</summary>
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
        public List<GridSortRequest>? Sorted { get; set; }
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

    /// <summary>Client + server validation for the templated "Admit resident" dialog.</summary>
    public class CareOpsAdmitValidator : ReactiveValidator<CareOpsViewModel>
    {
        public CareOpsAdmitValidator()
        {
            ClientRule(x => x.NewResidentName)
                .Required("Resident name is required.");

            ClientRule(x => x.NewResidentName)
                .MinLength(3, "Resident name must be at least 3 characters.");

            ClientRule(x => x.NewWing)
                .Required("Wing is required.");

            ClientRule(x => x.NewCareLevel)
                .Required("Care level is required.");

            ClientRule(x => x.NewRiskLevel)
                .Required("Risk level is required.");

            ClientRule(x => x.NewOpenTasks)
                .Required("Open tasks is required.");

            ClientRule(x => x.NewOpenTasks)
                .Range(0m, 12m, "Open tasks must be between 0 and 12.");
        }
    }

    /// <summary>
    /// Client + server validation for an in-cell care row edit. The same
    /// <c>ClientRule</c> metadata drives EJ2-native column validation in the grid
    /// (via <c>FusionGridValidation</c>) and a full server check on save.
    /// </summary>
    public class ResidentCareItemValidator : ReactiveValidator<ResidentCareItem>
    {
        public ResidentCareItemValidator()
        {
            ClientRule(r => r.OpenTasks)
                .Range(0, 7, "Open tasks must be between 0 and 7.");
        }
    }
}
