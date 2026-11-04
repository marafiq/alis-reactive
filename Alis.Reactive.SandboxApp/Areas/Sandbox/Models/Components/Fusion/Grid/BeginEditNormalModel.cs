namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CareReviewResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }

        public string HoldReason { get; set; } = "";
    }

    public class CareReviewCheckout
    {
        public int ResidentId { get; set; }

        public int CensusLine { get; set; }
    }

    public class CareReviewCheckedOut
    {
        public string Summary { get; set; } = "";
    }
}
