namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A resident on the Maple wing roster the pharmacy labels medications from.</summary>
    public class PharmacyRosterResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string PrimaryNurse { get; set; } = "";
    }

    /// <summary>A corrected name, sent to the pharmacy once the roster has saved it.</summary>
    public class PharmacyNameCorrection
    {
        public int ResidentId { get; set; }
        public string PreviousName { get; set; } = "";
        public string CorrectedName { get; set; } = "";

        /// <summary>The resident's line on the roster, counted from zero as the grid counts its rows.</summary>
        public int? RosterLine { get; set; }
    }

    /// <summary>What the pharmacy was told.</summary>
    public class PharmacyNotice
    {
        public string Summary { get; set; } = "";
    }
}
