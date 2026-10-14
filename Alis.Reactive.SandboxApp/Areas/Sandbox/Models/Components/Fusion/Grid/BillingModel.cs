using Alis.Reactive.FluentValidator;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The month-end billing board: the filters that narrow the board, and the add-charge dialog's fields.
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

    /// <summary>The choices the board's filters and the add-charge dialog offer.</summary>
    public static class MonthEndBillingOptions
    {
        public static readonly IReadOnlyList<string> CareLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" };
        public static readonly IReadOnlyList<string> Wings = new[] { "North", "East", "West", "South" };
        public static readonly IReadOnlyList<string> Statuses = new[] { "Pending", "Billed", "Paid" };
    }

    /// <summary>A resident's charges for the month, one row of the board.</summary>
    public class ResidentBillingItem
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string Wing { get; set; } = "";
        public decimal MonthlyRate { get; set; }
        public decimal AddOnCharges { get; set; }
        public decimal BalanceDue { get; set; }

        /// <summary>Pending (not yet billed), Billed, or Paid.</summary>
        public string BillingStatus { get; set; } = "";
    }

    /// <summary>
    /// One page of the board, as the grid takes it (<c>{result, count}</c>), and the sentence that says what
    /// the board holds and what is outstanding.
    /// </summary>
    public class ResidentBillingResponse
    {
        public List<ResidentBillingItem> Result { get; set; } = new();
        public int Count { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>The page of the board the grid asks for: its paging, its sort, and the board's filters.</summary>
    public class BillingDataRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<FusionGridSortColumn>? Sorted { get; set; }
        public string? FilterCareLevel { get; set; }
        public string? FilterWing { get; set; }
        public string? FilterStatus { get; set; }
    }

    /// <summary>The residents picked on the board, for a rate increase or to mark billed.</summary>
    public class BillingBulkRequest
    {
        public List<ResidentBillingItem>? SelectedRecords { get; set; }
        public decimal Percent { get; set; }
    }

    /// <summary>The rates and add-ons changed in the grid.</summary>
    public class BillingSaveRequest
    {
        public FusionGridBatchChanges<ResidentBillingItem>? BatchChanges { get; set; }
    }

    /// <summary>The board's filters, for the statements emailed to the families it holds.</summary>
    public class BillingStatementsRequest
    {
        public string? FilterCareLevel { get; set; }
        public string? FilterWing { get; set; }
        public string? FilterStatus { get; set; }
    }

    /// <summary>What the business office did, as the manager is told.</summary>
    public class BillingAnswer
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>
    /// The add-charge dialog's rules. Only the New* fields take part; the filters are not the dialog's.
    /// </summary>
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
