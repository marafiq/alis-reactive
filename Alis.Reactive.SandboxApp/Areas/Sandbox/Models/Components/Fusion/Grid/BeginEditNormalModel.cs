namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A resident on the wellness nurse's care review list, one row of the review grid.</summary>
    public class CareReviewResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public int OpenTasks { get; set; }

        /// <summary>Why the record is on hold, such as a hospital transfer; empty when it is not.</summary>
        public string HoldReason { get; set; } = "";
    }

    /// <summary>The record a nurse checks out by opening its review: the resident and their census line.</summary>
    public class CareReviewCheckout
    {
        public int ResidentId { get; set; }

        /// <summary>The resident's place on the census, counted from zero as the grid counts its rows.</summary>
        public int CensusLine { get; set; }
    }

    /// <summary>The server's word that the record is checked out to the nurse.</summary>
    public class CareReviewCheckedOut
    {
        public string Summary { get; set; } = "";
    }
}
