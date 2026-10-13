namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Selected care activities used by array-operator flows.</summary>
    public class ArrayOpsModel
    {
        public string[]? SelectedActivities { get; set; }
    }

    /// <summary>HTTP response carrying the resident object array used by the array DSL.</summary>
    public class ResidentRosterResponse
    {
        public ResidentRow[] Residents { get; set; } = System.Array.Empty<ResidentRow>();
    }

    /// <summary>Resident array element read by per-element predicates and selectors.</summary>
    public class ResidentRow
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Age { get; set; }
        public int Balance { get; set; }

        /// <summary>This month's charges, in dollars and cents.</summary>
        public decimal MonthlyCharges { get; set; }

        /// <summary>Hours of personal care a week.</summary>
        public double CareHours { get; set; }
    }
}
