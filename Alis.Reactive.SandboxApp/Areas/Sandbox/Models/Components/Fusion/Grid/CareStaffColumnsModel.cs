namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The morning huddle sheet the staffing scheduler arranges before it prints: whether it leads with each
    /// resident's risk, and what the community calls its wings. The grid holds the residents.
    /// </summary>
    public class HuddleSheetViewModel
    {
        /// <summary>Whether the risk column comes before the resident's name.</summary>
        public bool LeadWithRisk { get; set; }

        /// <summary>The heading of the wing column, in the community's own word for a wing.</summary>
        public string WingHeading { get; set; } = "";
    }

    /// <summary>A resident on the huddle sheet, one row of the sheet's grid.</summary>
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
