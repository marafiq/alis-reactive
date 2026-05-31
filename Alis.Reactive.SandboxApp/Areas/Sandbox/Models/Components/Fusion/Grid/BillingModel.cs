using System.Collections.Generic;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Page model for the resident month-end billing board.
    /// Carries the server-side filter panel inputs plus the templated "Add charge" dialog inputs.
    /// </summary>
    public class ResidentBillingViewModel
    {
        public string? FilterCareLevel { get; set; }
        public string? FilterWing { get; set; }
        public string? FilterStatus { get; set; }

        public string? NewResidentName { get; set; }
        public string? NewCareLevel { get; set; }
        public decimal? NewMonthlyRate { get; set; }
        public decimal? NewAddOnCharges { get; set; }
    }

    /// <summary>
    /// One resident's monthly billing row. Used both as the server store record and the grid DTO.
    /// </summary>
    public class ResidentBillingItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public decimal MonthlyRate { get; set; }
        public decimal AddOnCharges { get; set; }
        public decimal BalanceDue { get; set; }
        public string BillingStatus { get; set; } = "";
    }

    /// <summary>Server-side billing response. SF Grid custom binding expects {result, count}.</summary>
    public class ResidentBillingResponse
    {
        public List<ResidentBillingItem> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>One-row response for the templated dialog add and per-row server patches.</summary>
    public class ResidentBillingRowResponse
    {
        public ResidentBillingItem Row { get; set; } = new ResidentBillingItem();
        public string Summary { get; set; } = "";
    }

    public class BillingDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<GridSortRequest>? Sorted { get; set; }
        public List<string>? Group { get; set; }
        public string? FilterCareLevel { get; set; }
        public string? FilterWing { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class BillingBulkRequest
    {
        public List<ResidentBillingItem>? SelectedRecords { get; set; }
        public decimal? Percent { get; set; }
    }

    public class BillingSaveRequest
    {
        public FusionGridBatchChanges<ResidentBillingItem>? BatchChanges { get; set; }
    }

    /// <summary>
    /// Client + server validation for the templated "Add charge" dialog.
    /// Only the New* fields participate; the filter inputs are untouched.
    /// </summary>
    public class ResidentBillingAddValidator : ReactiveValidator<ResidentBillingViewModel>
    {
        public ResidentBillingAddValidator()
        {
            ClientRule(x => x.NewResidentName)
                .Required("Resident name is required.");

            ClientRule(x => x.NewResidentName)
                .MinLength(3, "Resident name must be at least 3 characters.");

            ClientRule(x => x.NewCareLevel)
                .Required("Care level is required.");

            ClientRule(x => x.NewMonthlyRate)
                .Required("Monthly rate is required.");

            ClientRule(x => x.NewMonthlyRate)
                .Range(0m, 20000m, "Monthly rate must be between 0 and 20,000.");

            ClientRule(x => x.NewAddOnCharges)
                .Required("Add-on charges are required.");

            ClientRule(x => x.NewAddOnCharges)
                .Range(0m, 5000m, "Add-on charges must be between 0 and 5,000.");
        }
    }
}
