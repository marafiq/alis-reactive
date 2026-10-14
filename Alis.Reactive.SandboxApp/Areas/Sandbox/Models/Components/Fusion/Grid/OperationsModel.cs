namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The charge nurse's clinical updates board: the clinical review team's decision for the resident picked
    /// on the board. The grid holds the residents.
    /// </summary>
    public class ClinicalUpdatesViewModel
    {
        public string Wing { get; set; } = "";

        /// <summary>The fall risk the review team set.</summary>
        public string? ReviewRisk { get; set; }

        /// <summary>The open tasks the review team left the resident with.</summary>
        public decimal? ReviewOpenTasks { get; set; }

        /// <summary>The fall risks a review can set.</summary>
        public List<string> RiskLevels { get; set; } = new();
    }

    /// <summary>A resident on the clinical updates board, one row of the board's grid.</summary>
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

    /// <summary>A resident's open tasks after the pharmacy's update.</summary>
    public class ClinicalUpdatesTaskCount
    {
        public int ResidentId { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>A resident's fall risk after the therapy team's update.</summary>
    public class ClinicalUpdatesFallRisk
    {
        public int ResidentId { get; set; }
        public string RiskLevel { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    /// <summary>The review team's decision and the resident picked on the board.</summary>
    public class ClinicalUpdatesReview
    {
        public string? ReviewRisk { get; set; }
        public decimal? ReviewOpenTasks { get; set; }
        public List<ClinicalUpdatesResident> Residents { get; set; } = new();
    }

    /// <summary>The resident as the review left them.</summary>
    public class ClinicalUpdatesReviewed
    {
        public ClinicalUpdatesResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>Why the review was not recorded.</summary>
    public class ClinicalUpdatesRefusal
    {
        public string Summary { get; set; } = "";
    }
}
