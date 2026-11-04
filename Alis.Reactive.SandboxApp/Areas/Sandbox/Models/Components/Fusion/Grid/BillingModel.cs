using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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

    public static class MonthEndBillingOptions
    {
        public static readonly IReadOnlyList<string> CareLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
        public static readonly IReadOnlyList<string> Wings = new[] { "North", "East", "West", "South" };
        public static readonly IReadOnlyList<string> Statuses = new[] { "Pending", "Billed", "Paid" };
    }

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

    public class ResidentBillingResponse : IFusionGridPage<ResidentBillingItem>
    {
        public List<ResidentBillingItem> Result { get; set; } = new();

        IReadOnlyList<ResidentBillingItem> IFusionGridPage<ResidentBillingItem>.Result => Result;
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    public class BillingDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<FusionGridSortColumn>? Sorted { get; set; }
        public string? FilterCareLevel { get; set; }
        public string? FilterWing { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class BillingBulkRequest
    {
        public List<ResidentBillingItem>? SelectedRecords { get; set; }
        public decimal Percent { get; set; }
    }

    public class BillingSaveRequest
    {
        public FusionGridBatchChanges<ResidentBillingItem>? BatchChanges { get; set; }
    }

    public class BillingStatementsRequest
    {
        public string? FilterCareLevel { get; set; }
        public string? FilterWing { get; set; }
        public string? FilterStatus { get; set; }
    }

    public class BillingAnswer
    {
        public string Summary { get; set; } = "";
    }

    public class ResidentBillingAddValidator : ReactiveValidator<ResidentBillingViewModel>
    {
        public ResidentBillingAddValidator()
        {
            RuleFor(x => x.NewResidentName)
                .NotEmpty().WithMessage("Enter the resident's name.")
                .MinimumLength(3).WithMessage("Write the resident's full name.");

            RuleFor(x => x.NewCareLevel).NotEmpty().WithMessage("Choose the resident's care level.");

            RuleFor(x => x.NewMonthlyRate)
                .NotNull().WithMessage("Enter the monthly rate.")
                .InclusiveBetween(0m, 20000m).WithMessage("A monthly rate runs from $0 to $20,000.");

            RuleFor(x => x.NewAddOnCharges)
                .NotNull().WithMessage("Enter the add-on charges, or 0 for none.")
                .InclusiveBetween(0m, 5000m).WithMessage("Add-on charges run from $0 to $5,000.");
        }
    }
}
