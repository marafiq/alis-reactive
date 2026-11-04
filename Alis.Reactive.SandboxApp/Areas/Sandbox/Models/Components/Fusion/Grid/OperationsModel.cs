namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ClinicalUpdatesViewModel
    {
        public string Wing { get; set; } = "";

        public string? ReviewRisk { get; set; }

        public decimal? ReviewOpenTasks { get; set; }

        public List<string> RiskLevels { get; set; } = new();
    }

    public class ClinicalUpdatesResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }
        public string PrimaryNurse { get; set; } = "";
        public string NextReview { get; set; } = "";
    }

    public class ClinicalUpdatesTaskCount
    {
        public int ResidentId { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }

    public class ClinicalUpdatesFallRisk
    {
        public int ResidentId { get; set; }
        public string RiskLevel { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    public class ClinicalUpdatesReview
    {
        public string? ReviewRisk { get; set; }
        public decimal? ReviewOpenTasks { get; set; }
        public List<ClinicalUpdatesResident> Residents { get; set; } = new();
    }

    public class ClinicalUpdatesReviewed
    {
        public ClinicalUpdatesResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class ClinicalUpdatesRefusal
    {
        public string Summary { get; set; } = "";
    }
}
