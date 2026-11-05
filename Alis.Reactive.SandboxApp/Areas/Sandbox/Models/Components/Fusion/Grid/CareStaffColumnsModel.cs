namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class HuddleSheetViewModel
    {
        public bool LeadWithRisk { get; set; }

        public string WingHeading { get; set; } = "";
    }

    public class HuddleSheetResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
        public string NextReview { get; set; } = "";
        public string Wing { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }
}
