using System.Collections.Generic;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Month-end billing state carries server-side filter inputs and the
    /// templated add-charge dialog inputs.
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

    /// <summary>Resident monthly billing row used as both server store record and grid DTO.</summary>
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

    /// <summary>Billing grid response shaped for Syncfusion custom binding: {result, count}.</summary>
    public class ResidentBillingResponse
    {
        public List<ResidentBillingItem> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>Single-row response for templated dialog adds and per-row server patches.</summary>
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
    /// Client and server validation for the templated add-charge dialog.
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
